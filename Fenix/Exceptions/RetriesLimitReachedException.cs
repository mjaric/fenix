namespace Fenix.Exceptions
{
    /// <summary>
    /// Exception thrown if the number of retries for an operation is reached.
    /// </summary>
     public class RetriesLimitReachedException : FinixException
    {
        /// <summary>
        /// Constructs a new instance of <see cref="RetriesLimitReachedException"/>.
        /// </summary>
        /// <param name="retries">The number of retries attempted.</param>
        public RetriesLimitReachedException(int retries) 
                : base($"Reached retries limit : {retries}")
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="RetriesLimitReachedException"/>.
        /// </summary>
        /// <param name="item">The name of the item for which retries were attempted.</param>
        /// <param name="retries">The number of retries attempted.</param>
        public RetriesLimitReachedException(string item, int retries)
                : base($"Item {item} reached retries limit : {retries}")
        {
        }
    }
}