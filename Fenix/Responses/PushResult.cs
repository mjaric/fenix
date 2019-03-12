using Newtonsoft.Json.Linq;

namespace Fenix.Responses
{
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