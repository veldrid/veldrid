namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetIndexBufferEntry : OpenGLCommandEntry
    {
        public DeviceBuffer Buffer;
        public IndexFormat Format;

        public SetIndexBufferEntry(DeviceBuffer buffer, IndexFormat format)
        {
            Buffer = buffer;
            Format = format;
        }

        public SetIndexBufferEntry() { }

        public SetIndexBufferEntry Init(DeviceBuffer buffer, IndexFormat format)
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