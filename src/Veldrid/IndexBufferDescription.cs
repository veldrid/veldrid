using System;

namespace Veldrid
{
    /// <summary>
    /// Describes an <see cref="IndexBuffer"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct IndexBufferDescription : IEquatable<IndexBufferDescription>
    {
        /// <summary>
        /// The desired capacity, in bytes, of the <see cref="IndexBuffer"/>.
        /// </summary>
        public ulong SizeInBytes;
        /// <summary>
        /// The format of index data to be stored in the <see cref="IndexBuffer"/>.
        /// </summary>
        public IndexFormat Format;
        /// <summary>
        /// A value indicating whether the <see cref="IndexBuffer"/> will be updated often.
        /// "false" should be used for static or rarely-changing data, and "true" should be used
        /// for data which will change continuously.
        /// </summary>
        public bool Dynamic;

        /// <summary>
        /// Constructs a new <see cref="IndexBuffer"/>.
        /// </summary>
        /// <param name="sizeInBytes">The desired capacity, in bytes, of the <see cref="IndexBuffer"/>.</param>
        /// <param name="format">The format of index data to be stored in the <see cref="IndexBuffer"/>.</param>
        public IndexBufferDescription(ulong sizeInBytes, IndexFormat format)
        {
            SizeInBytes = sizeInBytes;
            Format = format;
            Dynamic = false;
        }

        /// <summary>
        /// Constructs a new <see cref="IndexBuffer"/>.
        /// </summary>
        /// <param name="sizeInBytes">The desired capacity, in bytes, of the <see cref="IndexBuffer"/>.</param>
        /// <param name="format">The format of index data to be stored in the <see cref="IndexBuffer"/>.</param>
        /// <param name="dynamic">Whether the <see cref="IndexBuffer"/> should be specialized for continuous updates.</param>
        public IndexBufferDescription(ulong sizeInBytes, IndexFormat format, bool dynamic)
        {
            SizeInBytes = sizeInBytes;
            Format = format;
            Dynamic = dynamic;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(IndexBufferDescription other)
        {
            return SizeInBytes.Equals(other.SizeInBytes) && Format == other.Format && Dynamic.Equals(other.Dynamic);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(SizeInBytes.GetHashCode(), Format.GetHashCode(), Dynamic.GetHashCode());
        }
    }
}
