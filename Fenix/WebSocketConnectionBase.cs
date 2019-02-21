using System;
using System.Net.WebSockets;
using System.Threading;
using Fenix.Common;

namespace Fenix
{
    internal class WebSocketConnectionBase
    {
        public Uri Endpoint { get { return _endpoint; } }

        public bool IsInitialized => _socket != null;
        public bool IsClosed => IsInitialized && _socket.State == WebSocketState.Closed;
        public bool InSend => Interlocked.Read(ref _lastSendStarted) >= 0;
        public bool InReceive => Interlocked.Read(ref _lastReceiveStarted) >= 0;
        public int PendingSendBytes => _pendingSendBytes;
        public int InSendBytes => _inSendBytes;
        public int PendingReceivedBytes => _pendingReceivedBytes;
        public long TotalBytesSent => Interlocked.Read(ref _totalBytesSent);
        public long TotalBytesReceived => Interlocked.Read(ref _totalBytesReceived);
        public int SendCalls => _sentAsyncs;
        public int SendCallbacks => _sentAsyncCallbacks;
        public int ReceiveCalls => _recvAsyncs;
        public int ReceiveCallbacks => _recvAsyncCallbacks;

        public bool IsReadyForSend => !_isClosed;

        public bool IsReadyForReceive => !_isClosed;

        public bool IsFaulted => !_isClosed && _socket.CloseStatus != null;

        public DateTime? LastSendStarted
        {
            get
            {
                var ticks = Interlocked.Read(ref _lastSendStarted);
                return ticks >= 0 ? new DateTime(ticks) : (DateTime?)null;
            }
        }

        public DateTime? LastReceiveStarted
        {
            get
            {
                var ticks = Interlocked.Read(ref _lastReceiveStarted);
                return ticks >= 0 ? new DateTime(ticks) : (DateTime?)null;
            }
        }

        private ClientWebSocket _socket;
        private readonly Uri _endpoint;

        private long _lastSendStarted = -1;
        private long _lastReceiveStarted = -1;
        private volatile bool _isClosed;

        private int _pendingSendBytes;
        private int _inSendBytes;
        private int _pendingReceivedBytes;
        private long _totalBytesSent;
        private long _totalBytesReceived;

        private int _sentAsyncs;
        private int _sentAsyncCallbacks;
        private int _recvAsyncs;
        private int _recvAsyncCallbacks;

        public WebSocketConnectionBase(Uri endpoint)
        {
            Ensure.NotNull(endpoint, nameof(endpoint));
            _endpoint = endpoint;
        }

        protected void InitConnectionBase(ClientWebSocket socket)
        {
            Ensure.NotNull(socket, "socket");

            _socket = socket;
        }

        protected void NotifySendScheduled(int bytes)
        {
            Interlocked.Add(ref _pendingSendBytes, bytes);
        }

        protected void NotifySendStarting(int bytes)
        {
            if (Interlocked.CompareExchange(ref _lastSendStarted, DateTime.UtcNow.Ticks, -1) != -1)
                throw new Exception("Concurrent send detected.");
            Interlocked.Add(ref _pendingSendBytes, -bytes);
            Interlocked.Add(ref _inSendBytes, bytes);
            Interlocked.Increment(ref _sentAsyncs);
        }

        protected void NotifySendCompleted(int bytes)
        {
            Interlocked.Exchange(ref _lastSendStarted, -1);
            Interlocked.Add(ref _inSendBytes, -bytes);
            Interlocked.Add(ref _totalBytesSent, bytes);
            Interlocked.Increment(ref _sentAsyncCallbacks);
        }

        protected void NotifyReceiveStarting()
        {
            if (Interlocked.CompareExchange(ref _lastReceiveStarted, DateTime.UtcNow.Ticks, -1) != -1)
                throw new Exception("Concurrent receive detected.");
            Interlocked.Increment(ref _recvAsyncs);
        }

        protected void NotifyReceiveCompleted(int bytes)
        {
            Interlocked.Exchange(ref _lastReceiveStarted, -1);
            Interlocked.Add(ref _pendingReceivedBytes, bytes);
            Interlocked.Add(ref _totalBytesReceived, bytes);
            Interlocked.Increment(ref _recvAsyncCallbacks);
        }

        protected void NotifyReceiveDispatched(int bytes)
        {
            Interlocked.Add(ref _pendingReceivedBytes, -bytes);
        }

        protected void NotifyClosed()
        {
            _isClosed = true;
        }
    }
}