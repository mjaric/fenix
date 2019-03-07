using System;
using System.Runtime.Serialization;

namespace Fenix.Exceptions
{
    /// <summary>
    /// Exception thrown if there is no result for an operation for
    /// which one is expected.
    /// </summary>
    public class NoResultException : FenixException
    {
        /// <summary>
        /// Constructs a new <see cref="NoResultException"/>.
        /// </summary>
        public NoResultException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="NoResultException"/>.
        /// </summary>
        public NoResultException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="NoResultException"/>.
        /// </summary>
        public NoResultException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="NoResultException"/>.
        /// </summary>
        protected NoResultException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}