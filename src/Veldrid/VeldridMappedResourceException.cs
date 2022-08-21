using System;

namespace Veldrid
{
    /// <summary>
    /// Represents errors related to mapped resources.
    /// </summary>
    public class VeldridMappedResourceException : VeldridException
    {
        /// <summary>
        /// Constructs a new <see cref="VeldridMappedResourceException"/>.
        /// </summary>
        public VeldridMappedResourceException()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="VeldridMappedResourceException"/> with the given message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public VeldridMappedResourceException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="VeldridMappedResourceException"/> with the given message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public VeldridMappedResourceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
