using System;
using System.Runtime.Serialization;

namespace Fenix.Exceptions
{
    /// <summary>
    /// Exception thrown if an operation times out.
    /// </summary>
    public class OperationTimedOutException : FenixException
    {
        /// <summary>
        /// Constructs a new <see cref="OperationTimedOutException"/>.
        /// </summary>
        public OperationTimedOutException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="OperationTimedOutException"/>.
        /// </summary>
        public OperationTimedOutException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="OperationTimedOutException"/>.
        /// </summary>
        public OperationTimedOutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="OperationTimedOutException"/>.
        /// </summary>
        protected OperationTimedOutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}