namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetVertexBufferEntry
    {
        public readonly uint Index;
        public readonly HandleTracked<DeviceBuffer> Buffer;

        public NoAllocSetVertexBufferEntry(uint index, DeviceBuffer buffer)
        {
            Index = index;
            Buffer = new HandleTracked<DeviceBuffer>(buffer);
        }
    }
}