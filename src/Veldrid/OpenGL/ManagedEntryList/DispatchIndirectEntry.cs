namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class DispatchIndirectEntry : OpenGLCommandEntry
    {
        public Buffer IndirectBuffer;
        public uint Offset;

        public DispatchIndirectEntry() { }

        public DispatchIndirectEntry(Buffer indirectBuffer, uint offset)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
        }

        public DispatchIndirectEntry Init(Buffer indirectBuffer, uint offset)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;

            return this;
        }

        public override void ClearReferences()
        {
            IndirectBuffer = null;
        }
    }
}