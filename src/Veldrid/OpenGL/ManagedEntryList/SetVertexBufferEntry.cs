namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetVertexBufferEntry : OpenGLCommandEntry
    {
        public uint Index;
        public Buffer Buffer;

        public SetVertexBufferEntry(uint index, Buffer buffer)
        {
            Index = index;
            Buffer = buffer;
        }

        public SetVertexBufferEntry() { }

        public SetVertexBufferEntry Init(uint index, Buffer buffer)
        {
            Index = index;
            Buffer = buffer;
            return this;
        }

        public override void ClearReferences()
        {
            Buffer = null;
        }
    }
}