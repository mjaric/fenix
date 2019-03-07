using System;
using System.Threading;
using System.Threading.Tasks;
using Fenix.Common;
using Fenix.Exceptions;
using Fenix.Internal;
using Fenix.Logging;
using Newtonsoft.Json.Linq;

namespace Fenix.ClientOperations
{
    internal abstract class OperationBase<TResult> : IClientOperation
    {
        protected readonly ILogger Logger;
        private readonly string _requestEventType;
        private readonly TaskCompletionSource<TResult> _source;
        private long _joinRef;
        private readonly string _topic;
        private long _pushRef;

        private int _completed;

        private Push _response;

        public long JoinRef
        {
            get { return _joinRef; }
            protected set { _joinRef = value; }
        }

        public long PushRef => _pushRef;

        public string Topic => _topic;

        protected OperationBase(
            ILogger logger,
            TaskCompletionSource<TResult> source,
            long joinRef,
            string topic,
            string requestEventType
        )
        {
            Logger = logger;
            _source = source;
            _joinRef = joinRef;
            _topic = topic;
            _requestEventType = requestEventType;
        }

        protected abstract object CreateRequestDto();

        protected abstract InspectionResult InspectResponse(Push push);

        protected abstract TResult TransformResponse(Push push);


        public Push CreateNetworkPackage(long pushRef)
        {
            Ensure.Positive(pushRef, nameof(pushRef));
            _pushRef = pushRef;
            return new Push(
                _topic,
                _requestEventType,
                JObject.FromObject(CreateRequestDto()),
                _pushRef,
                _joinRef
            );
        }

        public virtual InspectionResult InspectPackage(Push push)
        {
            switch (push.ChannelEvent)
            {
                case ChannelEvents.Error:
                    // EXAMPLE:
                    // ["1","1","room:lobby","phx_error",{...}]
                    return InspectServerErrorResponse(push);
                case ChannelEvents.Close:
                    return InspectCloseResponse(push);
                default:
                    _response = push;
                    // EXAMPLE:
                    // ["1","1","room:lobby","phx_reply",{"status":"ok","response":{...}}]
                    // ["1","1","room:lobby","phx_reply",{"status":"error","response":{...}}]
                    // ["1","1","room:lobby","phx_close",{}]
                    return InspectResponse(push);
            }
        }

        protected void Succeed()
        {
            if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0)
            {
                if (_response != null)
                    _source.SetResult(TransformResponse(_response));
                else
                    _source.SetException(new NoResultException());
            }
        }

        public void Fail(Exception exception)
        {
            if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0)
            {
                _source.SetException(exception);
            }
        }

        /// <summary>
        /// By default, InspectServerErrorResponse will tell Operation manager to retry operation on server error.
        /// However, this method can be overwritten to return different <see cref="InspectionResult"/>
        /// </summary>
        /// <param name="push">instance of reply or error push</param>
        /// <returns>result that should tell <see cref="OperationsManager"/> what should be done with operation</returns>
        public virtual InspectionResult InspectServerErrorResponse(Push push)
        {
            Fail(new ServerErrorException(
                $"Push #{push.PushRef} failed, got `{push.ChannelEvent}` from phoenix server."));
            return new InspectionResult(InspectionDecision.Retry, "Server Error");
        }

        public InspectionResult InspectCloseResponse(Push push)
        {
            return new InspectionResult(InspectionDecision.EndOperation, "Got `phx_close` response.");
        }

        public InspectionResult InspectUnexpectedCommand(Push push)
        {
            Logger.Error($"Unexpected response from server, got channel event `{push.ChannelEvent}`");
            Fail(new UnexpectedServerResponse($"Unexpected server response `{push.ChannelEvent}`"));
            return new InspectionResult(InspectionDecision.EndOperation, "Unexpected event type");
        }
    }
}