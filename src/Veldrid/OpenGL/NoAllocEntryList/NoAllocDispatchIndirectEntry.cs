namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDispatchIndirectEntry
    {
        public HandleTracked<DeviceBuffer> IndirectBuffer;
        public uint Offset;

        public NoAllocDispatchIndirectEntry(DeviceBuffer indirectBuffer, uint offset)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
        }
    }
}