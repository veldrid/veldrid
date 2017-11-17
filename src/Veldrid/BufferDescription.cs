using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="Buffer"/>, used in the creation of <see cref="Buffer"/> objects by a
    /// <see cref="ResourceFactory"/>.
    /// </summary>
    public struct BufferDescription : IEquatable<BufferDescription>
    {
        /// <summary>
        /// The desired capacity, in bytes, of the <see cref="Buffer"/>.
        /// </summary>
        public ulong SizeInBytes;
        /// <summary>
        /// Indicates how the <see cref="Buffer"/> will be used.
        /// </summary>
        public BufferUsage Usage;
        /// <summary>
        /// A value indicating whether the <see cref="Buffer"/> will be updated often.
        /// "false" should be used for static or rarely-changing data, and "true" should be used for data which will change
        /// continuously.
        /// </summary>
        public bool Dynamic;

        /// <summary>
        /// Constructs a new <see cref="BufferDescription"/> describing a non-dynamic <see cref="Buffer"/>.
        /// </summary>
        /// <param name="sizeInBytes">The desired capacity, in bytes.</param>
        /// <param name="usage">Indicates how the <see cref="Buffer"/> will be used.</param>
        public BufferDescription(ulong sizeInBytes, BufferUsage usage)
        {
            SizeInBytes = sizeInBytes;
            Usage = usage;
            Dynamic = false;
        }

        /// <summary>
        /// Constructs a new <see cref="BufferDescription"/>.
        /// </summary>
        /// <param name="sizeInBytes">The desired capacity, in bytes.</param>
        /// <param name="usage">Indicates how the <see cref="Buffer"/> will be used.</param>
        /// <param name="dynamic">Whether the <see cref="Buffer"/> should be specialized for continuous updates.</param>
        public BufferDescription(ulong sizeInBytes, BufferUsage usage, bool dynamic)
        {
            SizeInBytes = sizeInBytes;
            Usage = usage;
            Dynamic = dynamic;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(BufferDescription other)
        {
            return SizeInBytes.Equals(other.SizeInBytes) && Dynamic.Equals(other.Dynamic);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(SizeInBytes.GetHashCode(), Dynamic.GetHashCode());
        }
    }
}
