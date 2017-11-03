namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class DrawEntry : OpenGLCommandEntry
    {
        public uint IndexCount;
        public uint InstanceCount;
        public uint IndexStart;
        public int VertexOffset;
        public uint InstanceStart;

        public DrawEntry(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            IndexCount = indexCount;
            InstanceCount = instanceCount;
            IndexStart = indexStart;
            VertexOffset = vertexOffset;
            InstanceStart = instanceStart;
        }

        public DrawEntry() { }

        public DrawEntry Init(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            IndexCount = indexCount;
            InstanceCount = instanceCount;
            IndexStart = indexStart;
            VertexOffset = vertexOffset;
            InstanceStart = instanceStart;
            return this;
        }

        public override void ClearReferences()
        {
        }
    }
}