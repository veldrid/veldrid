namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetVertexBufferEntry
    {
        public readonly uint Index;
        public readonly Tracked<DeviceBuffer> Buffer;
        public uint Offset;

        public NoAllocSetVertexBufferEntry(uint index, Tracked<DeviceBuffer> buffer, uint offset)
        {
            Index = index;
            Buffer = buffer;
            Offset = offset;
        }
    }
}
