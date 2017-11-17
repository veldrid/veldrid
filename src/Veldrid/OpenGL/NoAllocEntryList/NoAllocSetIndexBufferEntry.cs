namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetIndexBufferEntry
    {
        public readonly HandleTracked<Buffer> Buffer;
        public IndexFormat Format;

        public NoAllocSetIndexBufferEntry(Buffer ib, IndexFormat format)
        {
            Buffer = new HandleTracked<Buffer>(ib);
            Format = format;
        }
    }
}