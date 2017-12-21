namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDrawIndirectEntry
    {
        public HandleTracked<DeviceBuffer> IndirectBuffer;
        public uint Offset;
        public uint DrawCount;
        public uint Stride;

        public NoAllocDrawIndirectEntry(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;
        }
    }
}