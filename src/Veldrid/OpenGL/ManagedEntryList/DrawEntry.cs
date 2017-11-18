namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class DrawEntry : OpenGLCommandEntry
    {
        public uint VertexCount;
        public uint InstanceCount;
        public uint VertexStart;
        public uint InstanceStart;

        public DrawEntry() { }

        public DrawEntry(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            VertexCount = vertexCount;
            InstanceCount = instanceCount;
            VertexStart = vertexStart;
            InstanceStart = instanceStart;
        }

        public DrawEntry Init(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            VertexCount = vertexCount;
            InstanceCount = instanceCount;
            VertexStart = vertexStart;
            InstanceStart = instanceStart;

            return this;
        }

        public override void ClearReferences()
        {
        }
    }
}