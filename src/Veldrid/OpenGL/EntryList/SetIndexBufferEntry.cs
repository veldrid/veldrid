namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetIndexBufferEntry
    {
        public readonly Tracked<DeviceBuffer> Buffer;
        public IndexFormat Format;
        public uint Offset;

        public NoAllocSetIndexBufferEntry(Tracked<DeviceBuffer> ib, IndexFormat format, uint offset)
        {
            Buffer = ib;
            Format = format;
            Offset = offset;
        }
    }
}
