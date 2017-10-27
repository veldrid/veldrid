using System;
using System.Runtime.Serialization;

namespace Veldrid
{
    public class VeldridException : Exception
    {
        public VeldridException()
        {
        }

        public VeldridException(string message) : base(message)
        {
        }

        public VeldridException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VeldridException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
