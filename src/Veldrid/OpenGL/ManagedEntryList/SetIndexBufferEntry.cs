namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetIndexBufferEntry : OpenGLCommandEntry
    {
        public Buffer Buffer;
        public IndexFormat Format;

        public SetIndexBufferEntry(Buffer buffer, IndexFormat format)
        {
            Buffer = buffer;
            Format = format;
        }

        public SetIndexBufferEntry() { }

        public SetIndexBufferEntry Init(Buffer buffer, IndexFormat format)
        {
            Buffer = buffer;
            Format = format;
            return this;
        }

        public override void ClearReferences()
        {
            Buffer = null;
        }
    }
}