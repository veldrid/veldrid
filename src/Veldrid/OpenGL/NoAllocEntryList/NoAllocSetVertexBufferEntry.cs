namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetVertexBufferEntry
    {
        public readonly uint Index;
        public readonly HandleTracked<Buffer> Buffer;

        public NoAllocSetVertexBufferEntry(uint index, Buffer buffer)
        {
            Index = index;
            Buffer = new HandleTracked<Buffer>(buffer);
        }
    }
}