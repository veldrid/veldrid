namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetIndexBufferEntry
    {
        public readonly HandleTracked<DeviceBuffer> Buffer;
        public IndexFormat Format;

        public NoAllocSetIndexBufferEntry(DeviceBuffer ib, IndexFormat format)
        {
            Buffer = new HandleTracked<DeviceBuffer>(ib);
            Format = format;
        }
    }
}