using System;
using System.Runtime.Serialization;

namespace Fenix.Exceptions
{
    public class FinixException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="FinixException"/>.
        /// </summary>
        public FinixException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FinixException"/>.
        /// </summary>
        public FinixException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FinixException"/>.
        /// </summary>
        public FinixException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FinixException"/>.
        /// </summary>
        protected FinixException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}