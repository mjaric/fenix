using System;
using System.Threading.Tasks;
using Fenix.Exceptions;
using Fenix.Logging;
using Fenix.Responses;

namespace Fenix.ClientOperations
{
    internal class PushOperation : OperationBase<PushResult>
    {
        private readonly object _payload;

        public PushOperation(
            ILogger logger,
            TaskCompletionSource<PushResult> source,
            Channel channel,
            long joinRef,
            string topic,
            string requestEventType,
            object payload
        ) : base(logger, source, channel, joinRef, topic,
            requestEventType)
        {
            _payload = payload;
        }

        public void SetJoinRef(long joinRef)
        {
            JoinRef = joinRef;
        }
        protected override object CreateRequestDto()
        {
            return _payload;
        }

        protected override PushResult TransformResponse(Push push)
        {
            return push.Payload.ToObject<PushResult>();
        }

        protected override InspectionResult InspectResponse(Push push)
        {
            switch (push.ChannelEvent)
            {
                case ChannelEvents.Reply:
                    Succeed();
                    return new InspectionResult(InspectionDecision.EndOperation, "MessageSent");
                default:
                    Fail(new PushFailException(
                        $"Unexpected event type {push.ChannelEvent}, expected is {ChannelEvents.Reply}"));
                    return new InspectionResult(InspectionDecision.EndOperation, "UnexpectedEventType");
            }
        }
    }
}