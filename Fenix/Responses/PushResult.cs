using Newtonsoft.Json.Linq;

namespace Fenix.Responses
{
    /// <summary>
    /// Response received from server after push is sent to server inf ofr of reply. This object is constructed each time
    /// server respond on client push with event type "phx_reply".
    ///
    /// Status is either "ok" or "error", please check phoenix framework documentation for more details.
    /// Response is application response (response from phoenix application).
    ///
    /// Note: if status is "error", this means that it is application error, not infrastructure nor generic server error.
    /// </summary>
    public class PushResult
    {
        
        public readonly string Status;
        public readonly JObject Response;

        public PushResult(JObject response, string status)
        {
            Response = response;
            Status = status;
        }
    }
}