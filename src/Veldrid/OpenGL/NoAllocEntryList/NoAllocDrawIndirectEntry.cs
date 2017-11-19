namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDrawIndirectEntry
    {
        public HandleTracked<Buffer> IndirectBuffer;
        public uint Offset;
        public uint DrawCount;
        public uint Stride;

        public NoAllocDrawIndirectEntry(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;
        }
    }
}