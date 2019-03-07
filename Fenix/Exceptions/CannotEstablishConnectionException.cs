using System;
using System.Runtime.Serialization;

namespace Fenix.Exceptions
{
    public class CannotEstablishConnectionException : FenixException
    {
        /// <summary>
        /// Constructs a new <see cref="CannotEstablishConnectionException" />.
        /// </summary>
        public CannotEstablishConnectionException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CannotEstablishConnectionException" />.
        /// </summary>
        public CannotEstablishConnectionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CannotEstablishConnectionException" />.
        /// </summary>
        public CannotEstablishConnectionException(string message,
            Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CannotEstablishConnectionException" />.
        /// </summary>
        protected CannotEstablishConnectionException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    public class HeartbeatTimeoutException : FenixException
    {
        /// <summary>
        /// Constructs a new <see cref="CannotEstablishConnectionException" />.
        /// </summary>
        public HeartbeatTimeoutException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CannotEstablishConnectionException" />.
        /// </summary>
        public HeartbeatTimeoutException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CannotEstablishConnectionException" />.
        /// </summary>
        public HeartbeatTimeoutException(string message,
            Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CannotEstablishConnectionException" />.
        /// </summary>
        protected HeartbeatTimeoutException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}