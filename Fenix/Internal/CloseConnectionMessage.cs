using System;
using System.Net.WebSockets;

namespace Fenix.Internal
{
    internal class CloseConnectionMessage : InternalMessage
    {
        public readonly WebSocketCloseStatus Status;
        public readonly string Reason;
        public readonly Exception Exception;

        public CloseConnectionMessage(WebSocketCloseStatus status, string reason, Exception exception)
        {
            Reason = reason;
            Exception = exception;
            Status = status;
        }
    }

    internal class ConnectionClosedMessage : InternalMessage
    {
        public readonly WebSocketConnection Connection;
        public readonly WebSocketError Error;
        public readonly Exception Exception;

        public ConnectionClosedMessage(WebSocketConnection connection, WebSocketError error, Exception exception)
        {
            Connection = connection;
            Error = error;
            Exception = exception;
        }
    }
}