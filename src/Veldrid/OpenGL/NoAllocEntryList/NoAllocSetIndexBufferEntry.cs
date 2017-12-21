namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetIndexBufferEntry
    {
        public readonly Tracked<DeviceBuffer> Buffer;
        public IndexFormat Format;

        public NoAllocSetIndexBufferEntry(Tracked<DeviceBuffer> ib, IndexFormat format)
        {
            Buffer = ib;
            Format = format;
        }
    }
}