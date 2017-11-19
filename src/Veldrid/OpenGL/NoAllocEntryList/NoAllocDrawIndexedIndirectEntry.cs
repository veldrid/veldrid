namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDrawIndexedIndirectEntry
    {
        public HandleTracked<Buffer> IndirectBuffer;
        public uint Offset;
        public uint DrawCount;
        public uint Stride;

        public NoAllocDrawIndexedIndirectEntry(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;
        }
    }
}