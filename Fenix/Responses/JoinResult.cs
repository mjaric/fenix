using Newtonsoft.Json.Linq;

namespace Fenix.Responses
{
    public class JoinResult
    {
        public readonly string Status;
        public readonly JObject Response;

        public JoinResult(JObject response, string status)
        {
            Response = response;
            Status = status;
        }
    }
}