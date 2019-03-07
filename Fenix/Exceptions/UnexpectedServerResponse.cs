using System;
using System.Runtime.Serialization;

namespace Fenix.Exceptions
{
    public class UnexpectedServerResponse : FenixException
    {
        /// <summary>
        /// Constructs a new <see cref="UnexpectedServerResponse"/>.
        /// </summary>
        public UnexpectedServerResponse()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="UnexpectedServerResponse"/>.
        /// </summary>
        public UnexpectedServerResponse(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="UnexpectedServerResponse"/>.
        /// </summary>
        public UnexpectedServerResponse(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="UnexpectedServerResponse"/>.
        /// </summary>
        protected UnexpectedServerResponse(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}