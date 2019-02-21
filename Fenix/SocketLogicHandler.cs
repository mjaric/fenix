using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Fenix.ClientOperations;
using Fenix.Common;
using Fenix.Exceptions;
using Fenix.Internal;
using Fenix.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fenix
{
    internal class SocketLogicHandler
    {
        private static readonly TimerTickMessage TimerTickMessage = new TimerTickMessage();
        private readonly ILogger _logger;

        private readonly Socket _socket;
        private readonly Options _options;
        private WebSocketConnection _connection;
        private readonly SimpleQueuedHandler _queue;
        private readonly Timer _timer;

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private ReconnectionInfo _reconnInfo;
        private HeartbeatInfo _heartbeatInfo;
        private TimeSpan _lastTimeoutsTimeStamp;

        private ConnectionState _state = ConnectionState.Init;
        private ConnectingPhase _connectingPhase = ConnectingPhase.Invalid;

        private Uri _endpoint;
        private int _wasConnected;

        private long _packageNumber = 1;

//        private readonly OperationsManager _operations;

//        public int TotalOperationCount => _operations.TotalOperationCount;

        public SocketLogicHandler(Socket socket, Options options)
        {
            Ensure.NotNull(socket, nameof(socket));
            Ensure.NotNull(options, nameof(options));

            _socket = socket;
            _options = options;
            _logger = options.Logger;
            
            _queue = new SimpleQueuedHandler();

//            _operations = new OperationsManager(this, _options);

            //Connecting
            _queue.RegisterHandler<EstablishConnectionMessage>(msg =>
                          EstablishConnection(msg.Source, msg.Uri, msg.Parameters))
                  .RegisterHandler<CloseConnectionMessage>(msg =>
                          CloseConnection(msg.Status, msg.Reason, msg.Exception))
                  .RegisterHandler<ConnectionEstablishedMessage>(msg => ConnectionEstablished(msg.Connection))
                  .RegisterHandler<ConnectionClosedMessage>(msg =>
                          ConnectionClosed(msg.Connection, msg.Error, msg.Exception));

            _queue.RegisterHandler<JoinChannelMessage>(msg =>
                    JoinChannel(msg.Source, msg.Topic, msg.Parameters, msg.Timeout, msg.OnPush, msg.OnLeave));
            _queue.RegisterHandler<HandlePushMessage>(msg => HandlePush(msg.Connection, msg.RawMessage));

            _queue.RegisterHandler<TimerTickMessage>(_ => TimerTick());

            _timer = new Timer(_ => EnqueueMessage(TimerTickMessage), null, Consts.TimerPeriod, Consts.TimerPeriod);
        }

        public void EnqueueMessage(InternalMessage message)
        {
            if (message != TimerTickMessage)
                _logger.Debug($"Fenix: enqueueing message {message}");
            _queue.EnqueueMessage(message);
        }


        private void ConnectionEstablished(WebSocketConnection connection)
        {
            if (_state != ConnectionState.Connecting || _connection != connection || connection.IsClosed)
            {
                var oldConnId = _connection?.ConnectionId ?? Guid.Empty;
                var newConnId = connection.ConnectionId;

                _logger.Debug(
                    $"IGNORED (_state {_state}, _conn.Id {oldConnId:B}, conn.Id {newConnId:B}, conn.closed {connection.IsClosed}): WebSocket connection to [{connection.Endpoint}] established.");
                return;
            }
            _heartbeatInfo = new HeartbeatInfo(_packageNumber, true, _stopwatch.Elapsed);

            _logger.Debug(
                $"TCP connection to [{connection.Endpoint}, {connection.ConnectionId:B}] established.");
            _connectingPhase = ConnectingPhase.Connected;
            _state = ConnectionState.Connected;
        }

        private void CloseConnection(WebSocketCloseStatus status, string reason, Exception exception = null)
        {
            if (_state == ConnectionState.Closed)
            {
                _logger.Debug(
                    $"CloseConnection IGNORED because is WebSocketConnection is CLOSED, status {status}, reason {reason}, exception {exception}.");
                return;
            }

            _logger.Debug($"Finix: CloseConnection status {status}, reason {reason}, exception {exception}.");

            _state = ConnectionState.Closed;

            _timer.Dispose();
//            _operations.CleanUp();
//            _channels.CleanUp();
            CloseWebSocketConnection(status, reason);

            _logger.Info($"Finix: Connection Closed. Reason: {reason}.");

            if (exception != null)
                RaiseErrorOccurred(exception);

            RaiseClosed(reason);
        }

        private void ConnectionClosed(WebSocketConnection connection, WebSocketError error, Exception exception)
        {
        }

        private void CloseWebSocketConnection(WebSocketCloseStatus status, string reason)
        {
            if (_connection == null)
            {
                _logger.Debug("CloseWebSocketConnection IGNORED because _connection == null");
                return;
            }

            _logger.Debug("CloseTcpConnection");
            _connection.Close(status, reason);
            WebSocketConnectionClosed(_connection);
            _connection = null;
        }

        private void WebSocketConnectionClosed(WebSocketConnection connection)
        {
            if (_state == ConnectionState.Init) throw new Exception();
            if (_state == ConnectionState.Closed || _connection != connection)
            {
                var oldConnId = _connection?.ConnectionId ?? Guid.Empty;
                var newConnId = connection.ConnectionId;
                _logger.Debug(
                    $"IGNORED (_state {_state}, _conn.Id {oldConnId:B}, conn.Id {newConnId:B}, conn.closed {connection.IsClosed}): WebSocket connection to [{connection.Endpoint}] already closed.");
                return;
            }

            _state = ConnectionState.Connecting;
            _connectingPhase = ConnectingPhase.Reconnecting;

            _logger.Debug(
                "Finix: WebSocket connection to [{connection.Endpoint}, {connection.ConnectionId:B}] closed.");

            // _channels.PurgeSubscribedAndDroppedSubscriptions(_connection.ConnectionId);
            // _reconnInfo = new ReconnectionInfo(_reconnInfo.ReconnectionAttempt, _stopwatch.Elapsed);

            if (Interlocked.CompareExchange(ref _wasConnected, 0, 1) == 1)
            {
                RaiseDisconnected(connection.Endpoint);
            }
        }

        private void GoToConnectedState()
        {
            Ensure.NotNull(_connection, "_connection");

            _state = ConnectionState.Connected;
            _connectingPhase = ConnectingPhase.Connected;

            Interlocked.CompareExchange(ref _wasConnected, 1, 0);

            RaiseConnectedEvent(_connection.Endpoint);

            if (_stopwatch.Elapsed - _lastTimeoutsTimeStamp >= _options.OperationTimeoutCheckPeriod)
            {
//                _operations.CheckTimeoutsAndRetry(_connection);
//                _channels.CheckTimeoutsAndRetry(_connection);
                _lastTimeoutsTimeStamp = _stopwatch.Elapsed;
            }
        }

        private void HandlePush(WebSocketConnection connection, string rawMessage)
        {
            _logger.Debug($"Finix: received push `{rawMessage}`");
            var push = JArray.Parse(rawMessage);
            var joinRef = push.Value<string>(0);
            var pushRef = push.Value<string>(1);
            var channelName = push.Value<string>(2);
            var channelEvent = push.Value<string>(3);
            var payload = push.Value<JObject>(4);

            _packageNumber += 1;
            // this will reset heartbeat
            _heartbeatInfo = new HeartbeatInfo(_packageNumber, true, _stopwatch.Elapsed);
            
        }

        private void TimerTick()
        {
            switch (_state)
            {
                case ConnectionState.Init:
                case ConnectionState.Connecting:
                {
                    if (_connectingPhase == ConnectingPhase.Reconnecting &&
                        _stopwatch.Elapsed - _reconnInfo.TimeStamp >= _options.ReconnectionDelay)
                    {
                        _logger.Debug("TimerTick checking reconnection...");

                        _reconnInfo = new ReconnectionInfo(_reconnInfo.ReconnectionAttempt + 1, _stopwatch.Elapsed);
                        if (_options.MaxReconnections >= 0 &&
                            _reconnInfo.ReconnectionAttempt > _options.MaxReconnections)

                        {
                            const string reason = "Reconnection limit reached.";
                            CloseConnection(
                                WebSocketCloseStatus.EndpointUnavailable, 
                                reason,
                                new CannotEstablishConnectionException(reason)
                            );
                        }
                        else
                        {
                            RaiseReconnecting();
//                            _operations.CheckTimeoutsAndRetry(_connection);
                        }
                    }

                    if (_connectingPhase > ConnectingPhase.ConnectionEstablishing)
                        ManageHeartbeats();
                    break;
                }
                case ConnectionState.Connected:
                {
                    // operations timeouts are checked only if connection is established and check period time passed
                    if (_stopwatch.Elapsed - _lastTimeoutsTimeStamp >= _options.OperationTimeoutCheckPeriod)
                    {
                        // On mono even impossible connection first says that it is established
                        // so clearing of reconnection count on ConnectionEstablished event causes infinite reconnections.
                        // So we reset reconnection count to zero on each timeout check period when connection is established
                        _reconnInfo = new ReconnectionInfo(0, _stopwatch.Elapsed);
//                        _operations.CheckTimeoutsAndRetry(_connection);
//                        _subscriptions.CheckTimeoutsAndRetry(_connection);
                        _lastTimeoutsTimeStamp = _stopwatch.Elapsed;
                    }

                    ManageHeartbeats();
                    break;
                }
                case ConnectionState.Closed: break;
                default: throw new Exception($"Unknown state: {_state}.");
            }
        }

        private void ManageHeartbeats()
        {
            if (_connection == null) throw new Exception();

            var timeout = _heartbeatInfo.IsIntervalStage ? _options.HeartbeatInterval : _options.HeartbeatTimeout;
            if (_stopwatch.Elapsed - _heartbeatInfo.TimeStamp < timeout)
                return;

            
            // if some packages are sent to server, then we need to reset heartbeat to new check interval
            // so _packageNumber is not same as _heartbeatInfo.LastPackageNumber
            if (_heartbeatInfo.LastPackageNumber != _packageNumber)
            {
                _logger.Debug("Scheduling new heartbeat interval!!");
                _heartbeatInfo = new HeartbeatInfo(MakeRef(), true, _stopwatch.Elapsed);
                return;
            }

            if (_heartbeatInfo.IsIntervalStage)
            {
                _logger.Debug("Sending heartbeat!!");
                _connection.EnqueueSend(Push.CreateNetworkPackage("phoenix", "heartbeat", new object(), _heartbeatInfo.LastPackageNumber));
                _heartbeatInfo = new HeartbeatInfo(_heartbeatInfo.LastPackageNumber, false, _stopwatch.Elapsed);
            }
            else
            {
                _logger.Warn("Finix: Heartbeat timed out.");
                var msg =
                        $"Finix: closing TCP connection [{_connection.Endpoint}, {_connection.ConnectionId}] due to HEARTBEAT TIMEOUT at ref {_heartbeatInfo.LastPackageNumber}.";
                _logger.Warn(msg);
                CloseConnection(WebSocketCloseStatus.EndpointUnavailable, "Heartbeat timeout", new HeartbeatTimeoutException(msg));
            }
        }


        private void EstablishConnection(
            TaskCompletionSource<object> source,
            Uri uri,
            (string, string)[] parameters)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(uri, nameof(uri));

            switch (_state)
            {
                case ConnectionState.Init:
                    _state = ConnectionState.Connecting;
                    _connectingPhase = ConnectingPhase.Reconnecting;
                    _endpoint = uri;
                    Connect(source, _endpoint, parameters);
                    break;
                case ConnectionState.Connecting:
                case ConnectionState.Connected:
                {
                    source.SetException(
                        new InvalidOperationException("Phoenix websocket connection is already active."));
                    break;
                }
                case ConnectionState.Closed:
                    source.SetException(new ObjectDisposedException("Phoenix WebSocket"));
                    break;
                default:
                    throw new Exception($"Unknown state: {_state}");
            }
        }


        private void Connect(TaskCompletionSource<object> source, Uri uri, (string, string)[] parameters)
        {
            _logger?.Debug("Connecting...");
            if (_state != ConnectionState.Connecting) return;
            if (_connectingPhase != ConnectingPhase.Reconnecting) return;

            _connectingPhase = ConnectingPhase.ConnectionEstablishing;
            _connection = new WebSocketConnection(
                _logger,
                uri,
                parameters,
                TimeSpan.FromSeconds(20),
                (connection, str) => EnqueueMessage(new HandlePushMessage(connection, str)),
                connection => EnqueueMessage(new ConnectionEstablishedMessage(connection)),
                (connection, error, ex) => EnqueueMessage(new ConnectionClosedMessage(connection, error, ex))
            );
            _connection.ConnectAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    EnqueueMessage(
                        new CloseConnectionMessage(WebSocketCloseStatus.EndpointUnavailable,
                            $"Unable to connect endpoint {uri}", t.Exception));
                    source.SetException(
                        new CannotEstablishConnectionException($"Unable to connect endpoint {uri}", t.Exception));
                }
                else
                {
                    source.SetResult(null);
                }
            });
        }


        private void JoinChannel(
            TaskCompletionSource<Channel> source,
            string topic,
            object parameters,
            TimeSpan timeout,
            Action<Channel, Push> onPush,
            Action<Channel, ChannelLeaveReason, Exception> onLeave = null
        )
        {
            switch (_state)
            {
                case ConnectionState.Init:
                    source.SetException(new InvalidOperationException("Fenix socket connection is not active!"));
                    break;
                case ConnectionState.Connecting:
                case ConnectionState.Connected:
//                    var ops = new JoinChannelOperation(_logger, source, topic, onPush,
//                        onLeave, () => _connection);
                    // TODO: add Channel to ChannelManager
                    break;
                case ConnectionState.Closed:
                    source.SetException(new ObjectDisposedException("Finix websocket is closed and disposed."));
                    break;
                default: throw new Exception($"Unknown state: {_state}.");
            }
        }

        private void RaiseConnectedEvent(Uri endpoint)
        {
            Connected(_socket, new ClientConnectionEventArgs(_socket, endpoint));
        }

        private void RaiseDisconnected(Uri endpoint)
        {
            Disconnected(_socket, new ClientConnectionEventArgs(_socket, endpoint));
        }

        private void RaiseClosed(string reason)
        {
            Closed(_socket, new ClientClosedEventArgs(_socket, reason));
        }

        private void RaiseErrorOccurred(Exception exception)
        {
            ErrorOccurred(_socket, new ClientErrorEventArgs(_socket, exception));
        }

        private void RaiseReconnecting()
        {
            Reconnecting(_socket, new ClientReconnectingEventArgs(_socket));
        }


        public event EventHandler<ClientConnectionEventArgs> Connected = delegate { };
        public event EventHandler<ClientConnectionEventArgs> Disconnected = delegate { };
        public event EventHandler<ClientReconnectingEventArgs> Reconnecting = delegate { };
        public event EventHandler<ClientClosedEventArgs> Closed = delegate { };
        public event EventHandler<ClientErrorEventArgs> ErrorOccurred = delegate { };


        public long MakeRef()
        {
            return Interlocked.Increment(ref _packageNumber);
        }

        private struct HeartbeatInfo
        {
            public readonly long LastPackageNumber;
            public readonly bool IsIntervalStage;
            public readonly TimeSpan TimeStamp;

            public HeartbeatInfo(long lastPackageNumber, bool isIntervalStage, TimeSpan timeStamp)
            {
                LastPackageNumber = lastPackageNumber;
                IsIntervalStage = isIntervalStage;
                TimeStamp = timeStamp;
            }
        }

        private struct ReconnectionInfo
        {
            public readonly int ReconnectionAttempt;
            public readonly TimeSpan TimeStamp;

            public ReconnectionInfo(int reconnectionAttempt, TimeSpan timeStamp)
            {
                ReconnectionAttempt = reconnectionAttempt;
                TimeStamp = timeStamp;
            }
        }

        private enum ConnectionState
        {
            Init,
            Connecting,
            Connected,
            Closed
        }

        private enum ConnectingPhase
        {
            Invalid,
            Reconnecting,
            ConnectionEstablishing,
            Connected
        }
    }
}