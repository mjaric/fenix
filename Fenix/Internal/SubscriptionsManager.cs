//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using Fenix.ClientOperations;
//using Fenix.Common;
//using Fenix.Exceptions;
//
//namespace Fenix.Internal
//{
//    internal class SubscriptionItem
//    {
//        public readonly JoinChannelOperation Operation;
//        public readonly int MaxRetries;
//        public readonly TimeSpan Timeout;
//        public readonly DateTime CreatedTime;
//
//        public Guid ConnectionId;
//        public long PushRef;
//        public bool IsSubscribed;
//        public int RetryCount;
//        public DateTime LastUpdated;
//
//        public SubscriptionItem(JoinChannelOperation operation, int maxRetries, TimeSpan timeout)
//        {
//            Ensure.NotNull(operation, "operation");
//
//            Operation = operation;
//            MaxRetries = maxRetries;
//            Timeout = timeout;
//            CreatedTime = DateTime.UtcNow;
//            RetryCount = 0;
//            LastUpdated = DateTime.UtcNow;
//        }
//
//        public override string ToString()
//        {
//            return
//                    $"Subscription {Operation.GetType().Name} ({PushRef:D}): {Operation}, " +
//                    $"is subscribed: {IsSubscribed}, retry count: {RetryCount}, " +
//                    $"created: {CreatedTime:HH:mm:ss.fff}, last updated: {LastUpdated:HH:mm:ss.fff}";
//        }
//    }
//
//    internal class SubscriptionsManager
//    {
//        private readonly SocketLogicHandler _handler;
//        private readonly Settings _settings;
//
//        private readonly Dictionary<string, SubscriptionItem> _activeSubscriptions =
//                new Dictionary<string, SubscriptionItem>();
//
//        private readonly Dictionary<long, string> _activeSubscriptionsRefs =
//                new Dictionary<long, string>();
//
//        private readonly Queue<SubscriptionItem> _waitingSubscriptions = new Queue<SubscriptionItem>();
//        private readonly List<SubscriptionItem> _retryPendingSubscriptions = new List<SubscriptionItem>();
//
//        public SubscriptionsManager(SocketLogicHandler handler, Settings settings)
//        {
//            Ensure.NotNull(settings, nameof(settings));
//            Ensure.NotNull(handler, nameof(handler));
//            
//            _handler = handler;
//            _settings = settings;
//        }
//
//        public bool TryGetActiveSubscription(string topic, out SubscriptionItem subscription)
//        {
//            return _activeSubscriptions.TryGetValue(topic, out subscription);
//        }
//
//        public bool TryGetActiveSubscription(long pushRef, out SubscriptionItem subscriptionItem)
//        {
//            subscriptionItem = null;
//            return _activeSubscriptionsRefs.TryGetValue(pushRef, out var topic)
//                   && TryGetActiveSubscription(topic, out subscriptionItem);
//        }
//
//        public void CleanUp()
//        {
//            var connectionClosedException =
//                    new ConnectionClosedException($"Connection was closed.");
//            foreach (var subscription in _activeSubscriptions.Values
//                                                             .Concat(_waitingSubscriptions)
//                                                             .Concat(_retryPendingSubscriptions))
//            {
//                subscription.Operation.DropSubscription(SubscriptionDropReason.ConnectionClosed,
//                    connectionClosedException);
//            }
//
//            _activeSubscriptions.Clear();
//            _activeSubscriptionsRefs.Clear();
//            _waitingSubscriptions.Clear();
//            _retryPendingSubscriptions.Clear();
//        }
//
//        public void PurgeSubscribedAndDroppedSubscriptions(Guid connectionId)
//        {
//            var subscriptionsToRemove = new List<SubscriptionItem>();
//            foreach (var subscription in _activeSubscriptions.Values.Where(x =>
//                    x.IsSubscribed && x.ConnectionId == connectionId))
//            {
//                subscription.Operation.ConnectionClosed();
//                subscriptionsToRemove.Add(subscription);
//            }
//
//            foreach (var subscription in subscriptionsToRemove)
//            {
//                _activeSubscriptions.Remove(subscription.Operation.Topic);
//                _activeSubscriptionsRefs.Remove(subscription.PushRef);
//            }
//        }
//
//        public void CheckTimeoutsAndRetry(WebSocketConnection connection)
//        {
//            Ensure.NotNull(connection, "connection");
//
//            var retrySubscriptions = new List<SubscriptionItem>();
//            var removeSubscriptions = new List<SubscriptionItem>();
//            foreach (var subscription in _activeSubscriptions.Values)
//            {
//                if (subscription.IsSubscribed) continue;
//                if (subscription.ConnectionId != connection.ConnectionId)
//                {
//                    retrySubscriptions.Add(subscription);
//                }
//                else if (subscription.Timeout > TimeSpan.Zero &&
//                         DateTime.UtcNow - subscription.LastUpdated > _settings.OperationTimeout)
//                {
//                    var err =
//                            "WebSocketConnection: subscription never got confirmation from server.\n" +
//                            $"UTC now: {DateTime.UtcNow:HH:mm:ss.fff}, operation: {subscription}.";
//                    _settings.Logger.Error(err);
//
//                    if (_settings.FailOnNoServerResponse)
//                    {
//                        subscription.Operation.DropSubscription(SubscriptionDropReason.SubscribingError,
//                            new OperationTimedOutException(err));
//                        removeSubscriptions.Add(subscription);
//                    }
//                    else
//                    {
//                        retrySubscriptions.Add(subscription);
//                    }
//                }
//            }
//
//            foreach (var subscription in retrySubscriptions)
//            {
//                ScheduleSubscriptionRetry(subscription);
//            }
//
//            foreach (var subscription in removeSubscriptions)
//            {
//                RemoveSubscription(subscription);
//            }
//
//            if (_retryPendingSubscriptions.Count > 0)
//            {
//                foreach (var subscription in _retryPendingSubscriptions)
//                {
//                    subscription.RetryCount += 1;
//                    StartSubscription(subscription, connection);
//                }
//
//                _retryPendingSubscriptions.Clear();
//            }
//
//            while (_waitingSubscriptions.Count > 0)
//            {
//                StartSubscription(_waitingSubscriptions.Dequeue(), connection);
//            }
//        }
//
//        public bool RemoveSubscription(SubscriptionItem subscription)
//        {
//            var res = _activeSubscriptions.Remove(subscription.Operation.Topic);
//            _activeSubscriptionsRefs.Remove(subscription.Operation.PushRef);
//            LogDebug($"RemoveSubscription {subscription}, result {res}.");
//            return res;
//        }
//
//        public void ScheduleSubscriptionRetry(SubscriptionItem subscription)
//        {
//            if (!RemoveSubscription(subscription))
//            {
//                LogDebug($"RemoveSubscription failed when trying to retry {subscription}.");
//                return;
//            }
//
//            if (subscription.MaxRetries >= 0 && subscription.RetryCount >= subscription.MaxRetries)
//            {
//                LogDebug($"RETRIES LIMIT REACHED when trying to retry {subscription}.");
//                subscription.Operation.DropSubscription(SubscriptionDropReason.SubscribingError,
//                    new RetriesLimitReachedException(subscription.ToString(), subscription.RetryCount));
//                return;
//            }
//
//            LogDebug($"retrying subscription {subscription}.");
//            _retryPendingSubscriptions.Add(subscription);
//        }
//
//        public void EnqueueSubscription(SubscriptionItem subscriptionItem)
//        {
//            _waitingSubscriptions.Enqueue(subscriptionItem);
//        }
//
//        public void StartSubscription(SubscriptionItem subscription, WebSocketConnection connection)
//        {
//            Ensure.NotNull(connection, "connection");
//
//            if (subscription.IsSubscribed)
//            {
//                LogDebug($"StartSubscription REMOVING due to already subscribed {subscription}.");
//                RemoveSubscription(subscription);
//                return;
//            }
//
//            subscription.PushRef = _handler.MakeRef();
//            subscription.ConnectionId = connection.ConnectionId;
//            subscription.LastUpdated = DateTime.UtcNow;
//
//            _activeSubscriptions.Add(subscription.Operation.Topic, subscription);
//            _activeSubscriptionsRefs.Add(subscription.PushRef, subscription.Operation.Topic);
//            
//            if (!subscription.Operation.Subscribe(subscription.PushRef, connection))
//            {
//                LogDebug($"StartSubscription REMOVING AS COULD NOT SUBSCRIBE {subscription}.");
//                RemoveSubscription(subscription);
//            }
//            else
//            {
//                LogDebug($"StartSubscription SUBSCRIBING {subscription}.");
//            }
//        }
//
//        private void LogDebug(string message)
//        {
//            _settings.Logger.Debug($"Connection: {message}.");
//        }
//    }
//}