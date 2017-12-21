namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDispatchIndirectEntry
    {
        public Tracked<DeviceBuffer> IndirectBuffer;
        public uint Offset;

        public NoAllocDispatchIndirectEntry(Tracked<DeviceBuffer> indirectBuffer, uint offset)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
        }
    }
}