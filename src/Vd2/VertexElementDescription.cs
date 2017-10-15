namespace Vd2
{
    public struct VertexElementDescription
    {
        public string Name;
        public VertexElementFormat Format;
        public VertexElementSemantic Semantic;
        public uint InstanceStepRate; // 0 == per-vertex.

        public VertexElementDescription(string name, VertexElementFormat format, VertexElementSemantic semantic)
            : this(name, format, semantic, 0)
        {
        }

        public VertexElementDescription(
            string name,
            VertexElementFormat format,
            VertexElementSemantic semantic,
            uint instanceStepRate)
        {
            Name = name;
            Format = format;
            Semantic = semantic;
            InstanceStepRate = instanceStepRate;
        }
    }
}