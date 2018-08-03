using System;

namespace Veldrid
{
    /// <summary>
    /// Describes the layout of vertex data in a single <see cref="DeviceBuffer"/> used as a vertex buffer.
    /// </summary>
    public struct VertexLayoutDescription : IEquatable<VertexLayoutDescription>
    {
        /// <summary>
        /// The number of bytes in between successive elements in the <see cref="DeviceBuffer"/>.
        /// </summary>
        public uint Stride;
        /// <summary>
        /// An array of <see cref="VertexElementDescription"/> objects, each describing a single element of vertex data.
        /// </summary>
        public VertexElementDescription[] Elements;
        /// <summary>
        /// A value controlling how often data for instances is advanced for this layout. For per-vertex elements, this value
        /// should be 0.
        /// For example, an InstanceStepRate of 3 indicates that 3 instances will be drawn with the same value for this layout. The
        /// next 3 instances will be drawn with the next value, and so on.
        /// </summary>
        public uint InstanceStepRate;

        /// <summary>
        /// Constructs a new VertexLayoutDescription.
        /// </summary>
        /// <param name="stride">The number of bytes in between successive elements in the <see cref="DeviceBuffer"/>.</param>
        /// <param name="elements">An array of <see cref="VertexElementDescription"/> objects, each describing a single element
        /// of vertex data.</param>
        public VertexLayoutDescription(uint stride, params VertexElementDescription[] elements)
        {
            Stride = stride;
            Elements = elements;
            InstanceStepRate = 0;
        }

        /// <summary>
        /// Constructs a new VertexLayoutDescription.
        /// </summary>
        /// <param name="stride">The number of bytes in between successive elements in the <see cref="DeviceBuffer"/>.</param>
        /// <param name="elements">An array of <see cref="VertexElementDescription"/> objects, each describing a single element
        /// of vertex data.</param>
        /// <param name="instanceStepRate">A value controlling how often data for instances is advanced for this element. For
        /// per-vertex elements, this value should be 0.
        /// For example, an InstanceStepRate of 3 indicates that 3 instances will be drawn with the same value for this element.
        /// The next 3 instances will be drawn with the next value for this element, and so on.</param>
        public VertexLayoutDescription(uint stride, uint instanceStepRate, params VertexElementDescription[] elements)
        {
            Stride = stride;
            Elements = elements;
            InstanceStepRate = instanceStepRate;
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
                uint elementSize = FormatHelpers.GetSizeInBytes(elements[i].Format);
                if (elements[i].Offset != 0)
                {
                    computedStride = elements[i].Offset + elementSize;
                }
                else
                {
                    computedStride += elementSize;
                }
            }

            Stride = computedStride;
            InstanceStepRate = 0;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements and all array elements are equal; false otherswise.</returns>
        public bool Equals(VertexLayoutDescription other)
        {
            return Stride.Equals(other.Stride)
                && Util.ArrayEqualsEquatable(Elements, other.Elements)
                && InstanceStepRate.Equals(other.InstanceStepRate);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(Stride.GetHashCode(), HashHelper.Array(Elements), InstanceStepRate.GetHashCode());
        }
    }
}
