using System;

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
    }
}
