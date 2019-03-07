using System;
using System.Threading.Tasks;
using Fenix.Exceptions;
using Fenix.Logging;
using Fenix.Responses;
using Newtonsoft.Json.Linq;

namespace Fenix.ClientOperations
{
    internal class JoinChannelOperation : OperationBase<JoinResult>
    {
        public Channel Channel { get; }
        private readonly object _payload;

        public JoinChannelOperation(
            ILogger logger,
            TaskCompletionSource<JoinResult> source,
            Channel channel,
            long joinRef,
            object payload
        ) : base(logger, source, joinRef, channel.Topic, ChannelEvents.Join)
        {
            Channel = channel;
            _payload = payload;
        }

        protected override object CreateRequestDto()
        {
            return _payload;
        }

        protected override InspectionResult InspectResponse(Push push)
        {
            if (push.Payload.ContainsKey("status") && push.Payload.Value<string>("status") == "ok")
            {
                Succeed();
                Channel.Joined = true;
                return new InspectionResult(InspectionDecision.EndOperation, "Subscribed");
            }

            Fail(new JoinFailedException("Rejected", push.Payload["response"] as JObject));
            return new InspectionResult(InspectionDecision.EndOperation, "Server error");
        }

        protected override JoinResult TransformResponse(Push push)
        {
            return push.Payload.ToObject<JoinResult>();
        }

    }
}