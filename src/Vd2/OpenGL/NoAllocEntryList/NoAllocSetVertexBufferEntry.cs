namespace Vd2.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetVertexBufferEntry
    {
        public readonly uint Index;
        public readonly HandleTracked<VertexBuffer> VertexBuffer;

        public NoAllocSetVertexBufferEntry(uint index, VertexBuffer vb)
        {
            Index = index;
            VertexBuffer = new HandleTracked<VertexBuffer>(vb);
        }
    }
}