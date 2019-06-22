namespace Veldrid.CommandRecording
{
    internal struct NoAllocDrawEntry
    {
        public uint VertexCount;
        public uint InstanceCount;
        public uint VertexStart;
        public uint InstanceStart;

        public NoAllocDrawEntry(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            VertexCount = vertexCount;
            InstanceCount = instanceCount;
            VertexStart = vertexStart;
            InstanceStart = instanceStart;
        }
    }
}
