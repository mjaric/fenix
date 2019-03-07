using System;
using Newtonsoft.Json.Linq;

namespace Fenix
{
    public class Push
    {
        public readonly string Topic;
        public readonly string ChannelEvent;
        public readonly JObject Payload;
        public readonly long? PushRef;
        public readonly long? JoinRef;


        internal Push(string topic, string channelEvent, JObject payload, long? pushRef, long? joinRef = null)
        {
            Topic = topic;
            ChannelEvent = channelEvent;
            Payload = payload;
            PushRef = pushRef;
            JoinRef = joinRef;
        }

        internal string ToMessage()
        {
            var array = new JArray(
                JoinRef?.ToString(),
                PushRef?.ToString(),
                Topic,
                ChannelEvent,
                Payload
            );
            return array.ToString();
        }


        internal static string CreateNetworkPackage(string topic, string @event, object payload, long? @ref,
            long? joinRef = null)
        {
            // TODO: VSN 1 support
            return new Push(topic, @event, JObject.FromObject(payload), @ref, joinRef).ToMessage();
        }

        internal static Push FromPackage(string package)
        {
            // TODO: VSN 1 support
            var token = JArray.Parse(package);
            if (token.Count < 5)
            {
                throw new ArgumentException($"Got invalid payload from server `{package}`", nameof(package));
            }

            return new Push(
                joinRef: ParseRef(token[0].Value<string>()),
                pushRef: ParseRef(token[1].Value<string>()),
                topic: token[2].Value<string>(),
                channelEvent: token[3].Value<string>(),
                payload: token[4] as JObject
            );
        }

        public static long? ParseRef(string pushRef)
        {
            if (pushRef != null && long.TryParse(pushRef, out var refNumber))
            {
                return refNumber;
            }

            return null;
        }

        public string Inspect()
        {
            return $"#Push<JoinRef={JoinRef}, PushRef={PushRef}, Topic='{Topic}', ChannelEvent='{ChannelEvent}'>";
        }
    }
}