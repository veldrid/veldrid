using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="CommandList"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct CommandListDescription : IEquatable<CommandListDescription>
    {
        /// <summary>
        /// Command lists will be short, meaning that they will be reset or freed in a relatively short timeframe.
        /// This flag may be used by the implementation to control memory allocation behavior.
        /// </summary>
        public bool Transient;

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public readonly bool Equals(CommandListDescription other)
        {
            return Transient == other.Transient;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Transient);
        }
    }
}
