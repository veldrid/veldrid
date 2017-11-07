using System;

namespace Veldrid
{
    /// <summary>
    /// Describes the layout of vertex data in a single <see cref="VertexBuffer"/>.
    /// </summary>
    public struct VertexLayoutDescription : IEquatable<VertexLayoutDescription>
    {
        /// <summary>
        /// The number of bytes in between successive elements in the <see cref="VertexBuffer"/>.
        /// </summary>
        public uint Stride;
        /// <summary>
        /// An array of <see cref="VertexElementDescription"/> objects, each describing a single element of vertex data.
        /// </summary>
        public VertexElementDescription[] Elements;

        /// <summary>
        /// Constructs a new VertexLayoutDescription.
        /// </summary>
        /// <param name="stride">The number of bytes in between successive elements in the <see cref="VertexBuffer"/>.</param>
        /// <param name="elements">An array of <see cref="VertexElementDescription"/> objects, each describing a single element
        /// of vertex data.</param>
        public VertexLayoutDescription(uint stride, params VertexElementDescription[] elements)
        {
            Stride = stride;
            Elements = elements;
        }

        /// <summary>
        /// Constructs a new VertexLayoutDescription. The stride is assumed to be the sum of the size of all elements.
        /// </summary>
        /// <param name="elements">An array of <see cref="VertexElementDescription"/> objects, each describing a single element
        /// of vertex data.</param>
        public VertexLayoutDescription(params VertexElementDescription[] elements)
        {
            Elements = elements;
            uint computedStride = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                computedStride += FormatHelpers.GetSizeInBytes(elements[i].Format);
            }

            Stride = computedStride;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements and all array elements are equal; false otherswise.</returns>
        public bool Equals(VertexLayoutDescription other)
        {
            return Stride.Equals(other.Stride) && Util.ArrayEqualsEquatable(Elements, other.Elements);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(Stride.GetHashCode(), HashHelper.Array(Elements));
        }
    }
}
