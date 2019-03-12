using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace Fenix.Exceptions
{



    public class LeaveChannelFailedException : FenixException
    {
        public readonly JObject Response;
        /// <summary>
        /// Constructs a new <see cref="LeaveChannelFailedException"/>.
        /// </summary>
        public LeaveChannelFailedException(JObject response) : base()
        {
            Response = response;
        }

        /// <summary>
        /// Constructs a new <see cref="LeaveChannelFailedException"/>.
        /// </summary>
        public LeaveChannelFailedException(string message, JObject response) : base(message)
        {
            Response = response;
        }

        /// <summary>
        /// Constructs a new <see cref="LeaveChannelFailedException"/>.
        /// </summary>
        public LeaveChannelFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="LeaveChannelFailedException"/>.
        /// </summary>
        protected LeaveChannelFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }    
    /// <summary>
    /// Indicates that join to channel operation failed
    /// </summary>
    public class JoinFailedException : FenixException
    {
        public readonly JObject Response;
        /// <summary>
        /// Constructs a new <see cref="JoinFailedException"/>.
        /// </summary>
        public JoinFailedException(JObject response)
        {
            Response = response;
        }

        /// <summary>
        /// Constructs a new <see cref="JoinFailedException"/>.
        /// </summary>
        public JoinFailedException(string message, JObject response) : base(message)
        {
            Response = response;
        }

        /// <summary>
        /// Constructs a new <see cref="JoinFailedException"/>.
        /// </summary>
        public JoinFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="JoinFailedException"/>.
        /// </summary>
        protected JoinFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}