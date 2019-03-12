using System;
using System.Threading.Tasks;
using Fenix.Exceptions;
using Fenix.Logging;
using Fenix.Responses;
using Newtonsoft.Json.Linq;

namespace Fenix.ClientOperations
{
    internal class JoinChannelOperation : OperationBase<PushResult>
    {
        public Channel Channel { get; }
        private readonly object _payload;

        public JoinChannelOperation(
            ILogger logger,
            TaskCompletionSource<PushResult> source,
            Channel channel,
            long joinRef,
            object payload
        ) : base(logger, source, channel, joinRef, channel.Topic, ChannelEvents.Join)
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
                Channel.Open();
                return new InspectionResult(InspectionDecision.EndOperation, "Subscribed");
            }
            
            if (push.Payload.ContainsKey("status") && push.Payload.Value<string>("status") == "error")
            {
                Succeed();
                Channel.Close();
                return new InspectionResult(InspectionDecision.EndOperation, "Rejected");    
            }
            
            Fail(new JoinFailedException("UnexpectedResult", push.Payload["response"] as JObject));
            Channel.Close();
            return new InspectionResult(InspectionDecision.EndOperation, "UnexpectedResult");   
        }

        protected override PushResult TransformResponse(Push push)
        {
            return push.Payload.ToObject<PushResult>();
        }

    }
}