using System;

namespace Veldrid
{
    /// <summary>
    /// Represents errors that occur in the Veldrid library.
    /// </summary>
    public class VeldridException : Exception
    {
        /// <summary>
        /// Constructs a new VeldridException.
        /// </summary>
        public VeldridException()
        {
        }

        /// <summary>
        /// Constructs a new Veldridexception with the given message.
        /// </summary>
        /// <param name="message"></param>
        public VeldridException(string message) : base(message)
        {
        }
    }
}
