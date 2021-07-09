namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDrawIndexedIndirectEntry
    {
        public Tracked<DeviceBuffer> IndirectBuffer;
        public uint Offset;
        public uint DrawCount;
        public uint Stride;

        public NoAllocDrawIndexedIndirectEntry(Tracked<DeviceBuffer> indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;
        }
    }
}