namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetVertexBufferEntry : OpenGLCommandEntry
    {
        public uint Index;
        public VertexBuffer VertexBuffer;

        public SetVertexBufferEntry(uint index, VertexBuffer vb)
        {
            Index = index;
            VertexBuffer = vb;
        }

        public SetVertexBufferEntry() { }

        public SetVertexBufferEntry Init(uint index, VertexBuffer vb)
        {
            Index = index;
            VertexBuffer = vb;
            return this;
        }

        public override void ClearReferences()
        {
            VertexBuffer = null;
        }
    }
}