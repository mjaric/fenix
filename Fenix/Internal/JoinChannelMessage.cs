using System;
using System.Threading.Tasks;
using Fenix.Common;
using Fenix.Responses;
using Newtonsoft.Json.Linq;

namespace Fenix.Internal
{
    internal class JoinChannelMessage : InternalMessage
    {
        public readonly TaskCompletionSource<PushResult> Source;
        public readonly Channel Channel;
        public readonly string Topic;
        public readonly object Payload;

        public JoinChannelMessage(
            TaskCompletionSource<PushResult> source,
            Channel channel,
            string topic, 
            object payload
        )
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(topic, nameof(topic));
            Ensure.NotNull(topic, nameof(topic));
            
            Source = source;
            Channel = channel;
            Topic = topic;
            Payload = payload;
        }
    }

    internal class ChannelJoinedMessage : InternalMessage
    {
        public readonly long Ref;
        public readonly long JoinRef;
        public readonly string Topic;
        public readonly JObject JoinMessage;

        public ChannelJoinedMessage(long @ref, long joinRef, string topic, JObject joinMessage)
        {
            Ref = @ref;
            JoinRef = joinRef;
            Topic = topic;
            JoinMessage = joinMessage;
        }
    }
}