namespace Vd2
{
    // Inputs from a single VertexBuffer.
    public struct VertexLayoutDescription
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
    }
}
