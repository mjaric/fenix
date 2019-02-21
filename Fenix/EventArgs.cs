using System;

namespace Fenix
{
    /// <summary>
    /// Event Arguments for the event raised when an <see cref="Socket"/> is
    /// connected to or disconnected from an Phoenix server.
    /// </summary>
    public class ClientConnectionEventArgs : EventArgs
    {
        public Uri Endpoint { get; }
        public Socket Connection { get; }

        public ClientConnectionEventArgs(Socket connection, Uri endpoint)
        {
            Endpoint = endpoint;
            Connection = connection;
        }
    }
    
    /// <summary>
    /// Event Arguments for the event raised when an <see cref="Socket"/> is
    /// disconnected from an Phoenix server.
    /// </summary>
    public class ClientClosedEventArgs : EventArgs
    {
        /// <summary>
        /// A description of the reason the connection was closed if closing was initiated by the server or client API directly
        /// rather than by calling <see cref="Socket.Close"/>.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// The <see cref="Socket"/> responsible for raising the event.
        /// </summary>
        public Socket Connection { get; }

        /// <summary>
        /// Constructs a new instance of <see cref="ClientClosedEventArgs"/>.
        /// </summary>
        /// <param name="connection">The <see cref="Socket"/> responsible for raising the event.</param>
        /// <param name="reason">A description of the reason the connection was closed.</param>
        public ClientClosedEventArgs(Socket connection, string reason)
        {
            Connection = connection;
            Reason = reason;
        }
    }
    
    /// <summary>
    /// Event Arguments for the event raised when an error occurs on an <see cref="Socket"/>.
    /// </summary>
    public class ClientErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The thrown exception, if one was raised.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The <see cref="Socket"/> responsible for raising the event.
        /// </summary>
        public Socket Connection { get; }

        /// <summary>
        /// Constructs a new instance of <see cref="ClientErrorEventArgs"/>.
        /// </summary>
        /// <param name="connection">The <see cref="Socket"/> responsible for raising the event.</param>
        /// <param name="exception">The thrown exception, if one was raised.</param>
        public ClientErrorEventArgs(Socket connection, Exception exception)
        {
            Connection = connection;
            Exception = exception;
        }
    }
    
    /// <summary>
    /// Event Arguments for the event raised when an <see cref="Socket"/> is
    /// about to reconnect to an Phoenix server.
    /// </summary>
    public class ClientReconnectingEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="Socket"/> responsible for raising the event.
        /// </summary>
        public Socket Connection { get; }

        /// <summary>
        /// Constructs a new instance of <see cref="ClientReconnectingEventArgs"/>.
        /// </summary>
        /// <param name="connection">The <see cref="Socket"/> responsible for raising the event.</param>
        public ClientReconnectingEventArgs(Socket connection)
        {
            Connection = connection;
        }
    }
}