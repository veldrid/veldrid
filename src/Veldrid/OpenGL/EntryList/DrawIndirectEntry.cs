namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDrawIndirectEntry
    {
        public Tracked<DeviceBuffer> IndirectBuffer;
        public uint Offset;
        public uint DrawCount;
        public uint Stride;

        public NoAllocDrawIndirectEntry(Tracked<DeviceBuffer> indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;
        }
    }
}