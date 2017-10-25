namespace Vd2.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDrawEntry
    {
        public readonly uint IndexCount;
        public readonly uint InstanceCount;
        public readonly uint IndexStart;
        public readonly int VertexOffset;
        public readonly uint InstanceStart;

        public NoAllocDrawEntry(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            IndexCount = indexCount;
            InstanceCount = instanceCount;
            IndexStart = indexStart;
            VertexOffset = vertexOffset;
            InstanceStart = instanceStart;
        }
    }
}