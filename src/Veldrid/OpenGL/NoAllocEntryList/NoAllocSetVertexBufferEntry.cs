namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetVertexBufferEntry
    {
        public readonly uint Index;
        public readonly Tracked<DeviceBuffer> Buffer;

        public NoAllocSetVertexBufferEntry(uint index, Tracked<DeviceBuffer> buffer)
        {
            Index = index;
            Buffer = buffer;
        }
    }
}