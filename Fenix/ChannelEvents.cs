namespace Fenix
{
    public sealed class ChannelEvents
    {
        public const string Close = "phx_close";
        public const string Error = "phx_error";
        public const string Join = "phx_join";
        public const string Reply = "phx_reply";
        public const string Leave = "phx_leave";

        internal const string Heartbeat = "heartbeat";
    }
}