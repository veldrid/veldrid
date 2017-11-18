namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class DrawIndexedEntry : OpenGLCommandEntry
    {
        public uint IndexCount;
        public uint InstanceCount;
        public uint IndexStart;
        public int VertexOffset;
        public uint InstanceStart;

        public DrawIndexedEntry(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            IndexCount = indexCount;
            InstanceCount = instanceCount;
            IndexStart = indexStart;
            VertexOffset = vertexOffset;
            InstanceStart = instanceStart;
        }

        public DrawIndexedEntry() { }

        public DrawIndexedEntry Init(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
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