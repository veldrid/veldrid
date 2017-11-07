using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="CommandList"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct CommandListDescription : IEquatable<CommandListDescription>
    {
        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(CommandListDescription other)
        {
            return true;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}