using System;
using System.Runtime.Serialization;

namespace Vd2
{
    public class VdException : Exception
    {
        public VdException()
        {
        }

        public VdException(string message) : base(message)
        {
        }

        public VdException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VdException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
