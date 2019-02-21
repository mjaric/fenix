using Newtonsoft.Json.Linq;

namespace Fenix
{
    public class Push
    {
        public readonly string Topic;
        public readonly string Event;
        public readonly object Payload;
        public readonly long? Ref;
        public readonly long? JoinRef;


        public Push(string topic, string @event, object payload, long? @ref, long? joinRef = null)
        {
            Topic = topic;
            Event = @event;
            Payload = payload;
            Ref = @ref;
            JoinRef = joinRef;
        }

        internal string ToMessage()
        {
            var array = new JArray(
                JoinRef?.ToString(),
                Ref?.ToString(),
                Topic,
                Event,
                JObject.FromObject(Payload)
            );
            return array.ToString();
        }


        internal static string CreateNetworkPackage(string topic, string @event, object payload, long? @ref,
            long? joinRef = null)
        {
            return new Push(topic, @event, payload, @ref, joinRef).ToMessage();
        }
        

        private static long? ParseRef(string @ref)
        {
            if (@ref != null && long.TryParse(@ref, out var refNumber))
            {
                return refNumber;
            }

            return null;
        }
    }
}