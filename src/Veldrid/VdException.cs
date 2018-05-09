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
        /// <param name="message">The exception message.</param>
        public VeldridException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new Veldridexception with the given message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public VeldridException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
