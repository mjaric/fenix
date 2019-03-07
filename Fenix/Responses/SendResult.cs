using Newtonsoft.Json.Linq;

namespace Fenix.Responses
{
    public class SendResult
    {
        public readonly string Status;
        public readonly JObject Response;

        public SendResult(JObject response, string status)
        {
            Response = response;
            Status = status;
        }
    }
}