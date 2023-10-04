using System;

namespace Veldrid
{
    /// <summary>
    /// Represents errors related to lack of host or device memory.
    /// </summary>
    public class VeldridOutOfMemoryException : VeldridException
    {
        /// <summary>
        /// Constructs a new <see cref="VeldridOutOfMemoryException"/>.
        /// </summary>
        public VeldridOutOfMemoryException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="VeldridOutOfMemoryException"/> with the given message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public VeldridOutOfMemoryException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="VeldridOutOfMemoryException"/> with the given message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public VeldridOutOfMemoryException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
