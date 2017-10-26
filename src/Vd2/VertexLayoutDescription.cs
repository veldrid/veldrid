using System;

namespace Vd2
{
    // Inputs from a single VertexBuffer.
    public struct VertexLayoutDescription : IEquatable<VertexLayoutDescription>
    {
        public uint Stride;
        public VertexElementDescription[] Elements;

        public VertexLayoutDescription(uint stride, params VertexElementDescription[] elements)
        {
            Stride = stride;
            Elements = elements;
        }

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

        public bool Equals(VertexLayoutDescription other)
        {
            return Stride.Equals(other.Stride) && Util.ArrayEqualsEquatable(Elements, other.Elements);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(Stride.GetHashCode(), HashHelper.Array(Elements));
        }
    }
}
