using System.Threading.Tasks;
using Fenix.Exceptions;
using Fenix.Logging;
using Fenix.Responses;
using Newtonsoft.Json.Linq;

namespace Fenix.ClientOperations
{
    internal class LeaveChannelOperation : OperationBase<PushResult>
    {
        
        public Channel Channel { get; }
        public LeaveChannelOperation(
            ILogger logger,
            TaskCompletionSource<PushResult> source,
            Channel channel,
            long joinRef
        )
                : base(logger, source, channel, joinRef, channel.Topic, ChannelEvents.Leave)
        {
            Channel = channel;
        }

        protected override object CreateRequestDto()
        {
            return new {};
        }

        protected override InspectionResult InspectResponse(Push push)
        {
            if (push.Payload.ContainsKey("status"))
            {
                if (push.Payload.Value<string>("status") == "ok")
                {
                    Succeed();
                    Channel.Close();
                    return new InspectionResult(InspectionDecision.EndOperation, "ChannelClosed");    
                }

                Succeed();
                return new InspectionResult(InspectionDecision.EndOperation, "ChannelClosureError");

            }

            Fail(new LeaveChannelFailedException("Unexpected Response From Server", push.Payload["response"] as JObject));
            return new InspectionResult(InspectionDecision.EndOperation, "UnexpectedResponse");
        }

        protected override PushResult TransformResponse(Push push)
        {
            return push.Payload.ToObject<PushResult>();
        }
    }
}