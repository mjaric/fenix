using System;
using System.Runtime.Serialization;

namespace Fenix.Exceptions
{
    /// <summary>
    /// Exception thrown by ongoing operations which are terminated
    /// by an connection closing.
    /// </summary>
    public class ConnectionClosedException : FenixException
    {
        /// <summary>
        /// Constructs a new <see cref="ConnectionClosedException" />.
        /// </summary>
        public ConnectionClosedException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ConnectionClosedException" />.
        /// </summary>
        public ConnectionClosedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ConnectionClosedException" />.
        /// </summary>
        public ConnectionClosedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ConnectionClosedException" />.
        /// </summary>
        protected ConnectionClosedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}