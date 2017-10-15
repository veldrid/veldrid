namespace Vd2
{
    // Inputs from a single VertexBuffer.
    public struct VertexLayoutDescription
    {
        public VertexElementDescription[] Elements;

        public VertexLayoutDescription(params VertexElementDescription[] elements)
        {
            Elements = elements;
        }
    }
}
