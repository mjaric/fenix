using System.Threading.Tasks;
using Fenix.Exceptions;
using Fenix.Logging;
using Fenix.Responses;

namespace Fenix.ClientOperations
{
    internal class PushOperation : OperationBase<SendResult>
    {
        private object _payload;

        public PushOperation(
            ILogger logger,
            TaskCompletionSource<SendResult> source,
            long joinRef,
            string topic,
            string requestEventType,
            object payload
        ) : base(logger, source, joinRef, topic,
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

        protected override SendResult TransformResponse(Push push)
        {
            return push.Payload.ToObject<SendResult>();
        }

        protected override InspectionResult InspectResponse(Push push)
        {
            switch (push.ChannelEvent)
            {
                case ChannelEvents.Reply:
                    Succeed();
                    return new InspectionResult(InspectionDecision.EndOperation, "Subscribed");
                default:
                    Fail(new PushFailException(
                        $"Unexpected event type {push.ChannelEvent}, expected is {ChannelEvents.Reply}"));
                    return new InspectionResult(InspectionDecision.EndOperation, "Failed");
            }
        }
    }
}