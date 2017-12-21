namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class DispatchIndirectEntry : OpenGLCommandEntry
    {
        public DeviceBuffer IndirectBuffer;
        public uint Offset;

        public DispatchIndirectEntry() { }

        public DispatchIndirectEntry(DeviceBuffer indirectBuffer, uint offset)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
        }

        public DispatchIndirectEntry Init(DeviceBuffer indirectBuffer, uint offset)
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