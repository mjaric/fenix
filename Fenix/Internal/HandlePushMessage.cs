using System;

namespace Fenix.Internal
{
    internal class HandlePushMessage : InternalMessage
    {
        public readonly WebSocketConnection Connection;
        public readonly string RawMessage;

        public HandlePushMessage(WebSocketConnection connection, string rawMessage)
        {
            Connection = connection;
            RawMessage = rawMessage;
        }
    }


    internal class ConnectionErrorMessage : InternalMessage
    {
        public readonly WebSocketConnection Connection;
        public readonly Exception Exception;
        

        public ConnectionErrorMessage(WebSocketConnection connection, Exception exception)
        {
            Connection = connection;
            Exception = exception;
        }
    }
}