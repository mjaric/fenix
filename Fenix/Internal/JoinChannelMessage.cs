using System;
using System.Threading.Tasks;
using Fenix.Common;

namespace Fenix.Internal
{
    internal class JoinChannelMessage : InternalMessage
    {
        public readonly TaskCompletionSource<Channel> Source;
        public readonly string Topic;
        public readonly object Parameters;
        public readonly TimeSpan Timeout;
        public readonly Action<Channel, Push> OnPush;
        public readonly Action<Channel, ChannelLeaveReason, Exception> OnLeave;

        public JoinChannelMessage(
            TaskCompletionSource<Channel> source, 
            string topic, 
            object parameters,
            TimeSpan timeout,
            Action<Channel, Push> onPush,
            Action<Channel, ChannelLeaveReason, Exception> onLeave = null
        )
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(topic, nameof(topic));
            Ensure.NotNull(topic, nameof(topic));
            
            
            
            Source = source;
            Topic = topic;
            Parameters = parameters;
            Timeout = timeout;
            OnPush = onPush;
            OnLeave = onLeave;
        }
    }

    internal class ChannelJoinedMessage : InternalMessage
    {
        public readonly string ChannelName;
    }
}