using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Fenix.ClientOperations;
using Fenix.Common;
using Fenix.Exceptions;
using Fenix.Utils.Threading;

namespace Fenix.Internal
{
    internal class OperationItem
    {
        private static long _nextSeqNo = -1;
        public readonly long SeqNo = Interlocked.Increment(ref _nextSeqNo);

        public readonly IClientOperation Operation;
        public readonly int MaxRetries;
        public readonly TimeSpan Timeout;
        public readonly DateTime CreatedTime;

        public Guid ConnectionId;
        public long Ref;
        public int RetryCount;
        public DateTime LastUpdated;

        public OperationItem(IClientOperation operation, long pushRef, int maxRetries, TimeSpan timeout)
        {
            Ensure.NotNull(operation, "operation");

            Operation = operation;
            MaxRetries = maxRetries;
            Timeout = timeout;
            CreatedTime = DateTime.UtcNow;

            Ref = pushRef;
            RetryCount = 0;
            LastUpdated = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return
                    $"Operation {Operation.GetType()} ({Ref:D}): {Operation}, retry count: {RetryCount}, created: {CreatedTime:HH:mm:ss.fff}, last updated: {LastUpdated:HH:mm:ss.fff}";
        }
    }

    internal class OperationsManager
    {
        private static readonly IComparer<OperationItem> SeqNoComparer = new OperationItemSeqNoComparer();

        public int TotalOperationCount => _totalOperationCount;

        private readonly SocketLogicHandler _handler;
        private readonly Settings _settings;
        private readonly Dictionary<long, OperationItem> _activeOperations = new Dictionary<long, OperationItem>();

        private readonly ConcurrentQueueWrapper<OperationItem> _waitingOperations =
                new ConcurrentQueueWrapper<OperationItem>();

        private readonly List<OperationItem> _retryPendingOperations = new List<OperationItem>();
        private readonly object _lock = new object();
        private int _totalOperationCount;

        public OperationsManager(SocketLogicHandler handler, Settings settings)
        {
            Ensure.NotNull(handler, nameof(handler));
            Ensure.NotNull(settings, nameof(settings));
            _handler = handler;
            _settings = settings;
        }

        public bool TryGetActiveOperation(long correlationId, out OperationItem operation)
        {
            return _activeOperations.TryGetValue(correlationId, out operation);
        }

        public void CleanUp()
        {
            var connectionClosedException =
                    new ConnectionClosedException("Connection was closed.");
            foreach (var operation in _activeOperations.Values
                                                       .Concat(_waitingOperations)
                                                       .Concat(_retryPendingOperations))
            {
                operation.Operation.Fail(connectionClosedException);
            }

            _activeOperations.Clear();
            OperationItem dummy;
            while (_waitingOperations.TryDequeue(out dummy)) ;
            _retryPendingOperations.Clear();
            _totalOperationCount = 0;
        }

        public void CheckTimeoutsAndRetry(WebSocketConnection connection)
        {
            Ensure.NotNull(connection, nameof(connection));

            var retryOperations = new List<OperationItem>();
            var removeOperations = new List<OperationItem>();
            foreach (var operation in _activeOperations.Values)
            {
                if (operation.ConnectionId != connection.ConnectionId)
                {
                    retryOperations.Add(operation);
                }
                else if (operation.Timeout > TimeSpan.Zero &&
                         DateTime.UtcNow - operation.LastUpdated > _settings.OperationTimeout)
                {
                    var err = $"WebSocketConnection: operation never got response from server.\n" +
                              $"UTC now: {DateTime.UtcNow:HH:mm:ss.fff}, operation: {operation}.";
                    _settings.Logger.Debug(err);

                    if (_settings.FailOnNoServerResponse)
                    {
                        operation.Operation.Fail(new OperationTimedOutException(err));
                        removeOperations.Add(operation);
                    }
                    else
                    {
                        retryOperations.Add(operation);
                    }
                }
            }

            foreach (var operation in retryOperations)
            {
                ScheduleOperationRetry(operation);
            }

            foreach (var operation in removeOperations)
            {
                RemoveOperation(operation);
            }

            if (_retryPendingOperations.Count > 0)
            {
                _retryPendingOperations.Sort(SeqNoComparer);
                foreach (var operation in _retryPendingOperations)
                {
                    var oldPushRef = operation.Ref;
                    operation.Ref = _handler.MakeRef();
                    operation.RetryCount += 1;
                    LogDebug("retrying, old push ref {0}, operation {1}.", oldPushRef, operation);
                    ScheduleOperation(operation, connection);
                }

                _retryPendingOperations.Clear();
            }

            TryScheduleWaitingOperations(connection);
        }

        public void ScheduleOperationRetry(OperationItem operation)
        {
            if (!RemoveOperation(operation))
                return;

            LogDebug("ScheduleOperationRetry for {0}", operation);
            if (operation.MaxRetries >= 0 && operation.RetryCount >= operation.MaxRetries)
            {
                operation.Operation.Fail(new RetriesLimitReachedException(operation.ToString(), operation.RetryCount));
                return;
            }

            _retryPendingOperations.Add(operation);
        }

        public bool RemoveOperation(OperationItem operation)
        {
            if (!_activeOperations.Remove(operation.Ref))
            {
                LogDebug("RemoveOperation FAILED for {0}", operation);
                return false;
            }

            LogDebug("RemoveOperation SUCCEEDED for {0}", operation);
            _totalOperationCount = _activeOperations.Count + _waitingOperations.Count;
            return true;
        }

        public void TryScheduleWaitingOperations(WebSocketConnection connection)
        {
            Ensure.NotNull(connection, nameof(connection));
            lock (_lock)
            {
                // We don't want to transmit or retain expired requests, so we trim any from before the cutoff implied by the current time
                var cutoff = _settings.QueueTimeout == TimeSpan.Zero
                        ? (DateTime?) null
                        : DateTime.UtcNow - _settings.QueueTimeout;

                OperationItem operation;
                while (_activeOperations.Count < _settings.MaxConcurrentItems)
                {
                    if (!_waitingOperations.TryDequeue(out operation))
                        break;
                    if (cutoff == null || !TryExpireItem(cutoff.Value, operation))
                        ExecuteOperation(operation, connection);
                }

                if (cutoff != null)
                {
                    // In case the active operations queue is at capacity, we trim expired items from the front of the queue
                    while (_waitingOperations.TryPeek(out operation) && TryExpireItem(cutoff.Value, operation))
                    {
                        _waitingOperations.TryDequeue(out operation);
                    }
                }

                _totalOperationCount = _activeOperations.Count + _waitingOperations.Count;
            }
        }

        bool TryExpireItem(DateTime cutoffDate, OperationItem operation)
        {
            if (operation.CreatedTime > cutoffDate)
                return false;

            var err = $"WebSocketConnection: request expired.\n" +
                      $"UTC now: {DateTime.UtcNow:HH:mm:ss.fff}, operation: {operation}.";
            _settings.Logger.Debug(err);
            operation.Operation.Fail(new OperationExpiredException(err));
            return true;
        }

        private void ExecuteOperation(OperationItem operation, WebSocketConnection connection)
        {
            operation.ConnectionId = connection.ConnectionId;
            operation.LastUpdated = DateTime.UtcNow;
            _activeOperations.Add(operation.Ref, operation);

            var package = operation.Operation.CreateNetworkPackage(operation.Ref);
            LogDebug("ExecuteOperation push {0}, {1}, {2}.", package.ChannelEvent, package.PushRef, operation);
            connection.EnqueueSend(package.ToMessage());
        }

        public void EnqueueOperation(OperationItem operation)
        {
            LogDebug("EnqueueOperation WAITING for {0}.", operation);
            _waitingOperations.Enqueue(operation);
        }

        public void ScheduleOperation(OperationItem operation, WebSocketConnection connection)
        {
            Ensure.NotNull(connection, "connection");
            _waitingOperations.Enqueue(operation);
            TryScheduleWaitingOperations(connection);
        }

        private void LogDebug(string message, params object[] parameters)
        {
            _settings.Logger.Debug(
                $"WebSocketConnection: {(parameters.Length == 0 ? message : string.Format(message, parameters))}."
            );
        }

        private class OperationItemSeqNoComparer : IComparer<OperationItem>
        {
            public int Compare(OperationItem x, OperationItem y)
            {
                if (x == null) return 1;
                if (y == null) return -1;
                return x.SeqNo.CompareTo(y.SeqNo);
            }
        }
    }
}