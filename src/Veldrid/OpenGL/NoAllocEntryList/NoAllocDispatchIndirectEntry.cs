namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDispatchIndirectEntry
    {
        public HandleTracked<Buffer> IndirectBuffer;
        public uint Offset;

        public NoAllocDispatchIndirectEntry(Buffer indirectBuffer, uint offset)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
        }
    }
}