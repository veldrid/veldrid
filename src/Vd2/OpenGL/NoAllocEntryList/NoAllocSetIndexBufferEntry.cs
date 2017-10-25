namespace Vd2.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetIndexBufferEntry
    {
        public readonly HandleTracked<IndexBuffer> IndexBuffer;

        public NoAllocSetIndexBufferEntry(IndexBuffer ib)
        {
            IndexBuffer = new HandleTracked<IndexBuffer>(ib);
        }
    }
}