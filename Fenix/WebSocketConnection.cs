using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fenix.Common;
using Fenix.Logging;
using Fenix.Utils;
using Fenix.Utils.Threading;

namespace Fenix
{
    internal class WebSocketConnection : WebSocketConnectionBase
    {
        private readonly ILogger _logger;
        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private readonly ClientWebSocket _ws;
        private readonly Uri _uri;
        private readonly (string, string)[] _params;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private readonly Action<WebSocketConnection> _onConnected;
        private readonly Action<WebSocketConnection, string> _onMessage;
        private readonly Action<WebSocketConnection, WebSocketError, Exception> _onDisconnected;

        public readonly Guid ConnectionId;

        private readonly ConcurrentQueueWrapper<ArraySegment<byte>>
                _sendQueue = new ConcurrentQueueWrapper<ArraySegment<byte>>();

        private readonly ConcurrentQueueWrapper<IEnumerable<ArraySegment<byte>>> _receiveQueue =
                new ConcurrentQueueWrapper<IEnumerable<ArraySegment<byte>>>();


        private int _sending;
        private int _receiving;


        public WebSocketConnection(
            ILogger logger,
            Uri uri,
            (string, string)[] parameters,
            TimeSpan keepAliveInterval,
            Action<WebSocketConnection, string> handleMessage,
            Action<WebSocketConnection> connectionEstablished,
            Action<WebSocketConnection, WebSocketError, Exception> connectionClosed
        ) : base(uri)
        {
            Ensure.NotNull(logger, nameof(logger));
            Ensure.NotNull(uri, nameof(uri));
            Ensure.Positive(keepAliveInterval.TotalMilliseconds, nameof(keepAliveInterval));

            ConnectionId = Guid.NewGuid();


            _logger = logger;
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = keepAliveInterval;
            InitConnectionBase(_ws);
            _uri = uri;
            _params = parameters;
            _cancellationToken = _cancellationTokenSource.Token;

            _onMessage = handleMessage;
            _onConnected = connectionEstablished;
            _onDisconnected = connectionClosed;
        }

        internal void EnqueueSend(string message)
        {
            _logger.Debug(message);
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            _sendQueue.Enqueue(new ArraySegment<byte>(messageBuffer, 0, messageBuffer.Length));
            NotifySendScheduled(messageBuffer.Length);
            TrySend();
        }


        private async void SendMessageAsync(ArraySegment<byte> message)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
            }

            if (message.Array == null) return;
            var length = message.Count;
            var messagesCount = (int) Math.Ceiling((double) length / SendChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = SendChunkSize * i;
                var count = SendChunkSize;
                var lastMessage = i + 1 == messagesCount;

                if (count * (i + 1) > message.Array.Length)
                {
                    count = message.Array.Length - offset;
                }

                await _ws.SendAsync(new ArraySegment<byte>(message.Array, offset, count), WebSocketMessageType.Text,
                    lastMessage, CancellationToken.None);
            }

            NotifySendCompleted(message.Count);
        }


        private void TrySend()
        {
            while (!_sendQueue.IsEmpty && Interlocked.CompareExchange(ref _sending, 1, 0) == 0)
            {
                while (_sendQueue.TryDequeue(out var segment))
                {
                    if (segment.Array == null) continue;
                    NotifySendStarting(segment.Count);
                    SendMessageAsync(segment);
                }

                Interlocked.Exchange(ref _sending, 0);
            }
        }

        public Task ConnectAsync()
        {
            var uri = new Uri(_uri, "");

            foreach (var (key, value) in _params)
            {
                uri = uri.AddQuery(key, value);
            }

            // TODO: this should be configurable, so we can use vsn=1 too.
            uri = uri.AddQuery("vsn", "2.0.0");

            return _ws.ConnectAsync(uri, new CancellationTokenSource(10000).Token)
                      .ContinueWith(task =>
                      {
                          if (task.IsFaulted)
                          {
                              CallOnDisconnected(WebSocketError.ConnectionClosedPrematurely, task.Exception);
                              if (task.Exception != null)
                              {
                                  throw task.Exception;
                              }

                              return;
                          }

                          CallOnConnected();
                          StartListen();
                      }, TaskContinuationOptions.RunContinuationsAsynchronously);
        }

        public void Close(WebSocketCloseStatus status, string reason)
        {
            if (_ws == null)
                throw new InvalidOperationException("Failed connection.");
            if (_ws.State != WebSocketState.Closed)
                _ws.CloseAsync(status, reason, _cancellationToken);
        }

        private async void StartListen()
        {
            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var segments = new List<ArraySegment<byte>>();

                    WebSocketReceiveResult result;
                    do
                    {
                        var buffer = new byte[ReceiveChunkSize];
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer),
                            new CancellationTokenSource(int.MaxValue).Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _ws.CloseAsync(
                                WebSocketCloseStatus.NormalClosure,
                                string.Empty,
                                CancellationToken.None
                            );
                            CallOnDisconnected(WebSocketError.ConnectionClosedPrematurely);
                        }
                        else if (result.Count > 0)
                        {
                            segments.Add(new ArraySegment<byte>(buffer, 0, result.Count));
                        }
                    } while (!result.EndOfMessage && _ws.State == WebSocketState.Open);

                    if (result.MessageType != WebSocketMessageType.Close)
                    {
                        ProcessReceive(segments);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                CallOnDisconnected(WebSocketError.Faulted, ex);
            }
            finally
            {
                _ws.Dispose();
            }
        }

        private void ProcessReceive(IEnumerable<ArraySegment<byte>> segments)
        {
            var enumerable = segments as ArraySegment<byte>[] ?? segments.ToArray();
            Ensure.NotNull(enumerable, nameof(segments));

            var length = enumerable.Sum(s => s.Count);
            NotifyReceiveCompleted(length);
            _receiveQueue.Enqueue(enumerable);
            TryDequeueReceivedData();
        }

        private void TryDequeueReceivedData()
        {
            while (!_receiveQueue.IsEmpty && Interlocked.CompareExchange(ref _receiving, 1, 0) == 0)
            {
                if (!_receiveQueue.IsEmpty)
                {
                    _receiveQueue.TryDequeue(out var segments);

                    var enumerable = segments as ArraySegment<byte>[] ?? segments.ToArray();
                    var bytes = enumerable.Sum(s => s.Count);
                    using (var stream = new MemoryStream(new byte[bytes], 0, bytes, true))
                    {
                        var offset = 0;
                        foreach (var segment in enumerable)
                        {
                            stream.Write(segment.Array, offset, offset + segment.Count);
                        }

                        var message = Encoding.UTF8.GetString(stream.ToArray());
                        CallOnMessage(new StringBuilder(message));
                    }

                    NotifyReceiveDispatched(bytes);
                }

                Interlocked.Exchange(ref _receiving, 0);
            }
        }


        private void CallOnMessage(StringBuilder stringResult)
        {
            if (_onMessage != null)
                RunInTask(() => _onMessage(this, stringResult.ToString()));
        }

        private void CallOnDisconnected(WebSocketError error, Exception ex = null)
        {
            if (_onDisconnected != null)
                RunInTask(() => _onDisconnected(this, error, ex));
        }

        private void CallOnConnected()
        {
            if (_onConnected != null)
                RunInTask(() => _onConnected(this));
        }

        private static void RunInTask(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}