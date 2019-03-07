namespace Fenix.Exceptions
{
    /// <summary>
    /// Indicates that join operation has failed due to join timeout has been reached.
    /// </summary>
    public class JoinTimoutException : FenixException
    {
    }

    /// <summary>
    /// Indicates that channel errored
    /// </summary>
    public class ChannelError : FenixException
    {
    }
}