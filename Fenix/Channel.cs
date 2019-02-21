using System;
using Fenix.ClientOperations;

namespace Fenix
{
    /// <summary>
    /// Represents a subscription to single phoenix channel.
    /// </summary>
    public class Channel : IDisposable
    {

        private readonly Socket _socket;
        private readonly object _params;
        private ChannelState _state;
        
        /// <summary>
        /// Phoenix channel name to which this subscription is joined.
        /// </summary>
        public string Topic { get; private set; }


        internal Channel(
            string topic,
            Socket socket
        )
        {
            Topic = topic;
            _socket = socket;
            _state = ChannelState.Closed;
        }


        public void Dispose()
        {
            Leave();
        }

        public void Leave()
        {
//            _joinOperation.Unsubscribe();
        }

        internal string ReplayEventName(long pushRef)
        {
            return $"chan_reply_{pushRef}";
        }
    }

    internal enum ChannelState
    {
        Closed,
        Errored,
        Joined,
        Joining,
    }

    public enum ChannelLeaveReason
    {
        UserInitiated,
        AccessDenied,
        ServerError,
        ConnectionError
    }
    
    public sealed class ChannelEvents
    {
        public const string Close = "phx_close";
        public const string Error = "phx_error";
        public const string Join = "phx_join";
        public const string Reply = "phx_reply";
        public const string Leave = "phx_leave";

        internal const string Heartbeat = "heartbeat";
    }
}