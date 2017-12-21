namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetVertexBufferEntry : OpenGLCommandEntry
    {
        public uint Index;
        public DeviceBuffer Buffer;

        public SetVertexBufferEntry(uint index, DeviceBuffer buffer)
        {
            Index = index;
            Buffer = buffer;
        }

        public SetVertexBufferEntry() { }

        public SetVertexBufferEntry Init(uint index, DeviceBuffer buffer)
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