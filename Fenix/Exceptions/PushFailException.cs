namespace Fenix.Exceptions
{
    public class PushFailException: FenixException
    {
        public PushFailException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="PushFailException"/>.
        /// </summary>
        public PushFailException(string message) : base(message)
        {
        }
    }
}