using System;
using System.Runtime.Serialization;

namespace Fenix.Exceptions
{
    public class FenixException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="FenixException"/>.
        /// </summary>
        public FenixException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FenixException"/>.
        /// </summary>
        public FenixException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FenixException"/>.
        /// </summary>
        public FenixException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FenixException"/>.
        /// </summary>
        protected FenixException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}