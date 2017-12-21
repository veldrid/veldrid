namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class DrawIndirectEntry : OpenGLCommandEntry
    {
        public DeviceBuffer IndirectBuffer;
        public uint Offset;
        public uint DrawCount;
        public uint Stride;

        public DrawIndirectEntry() { }

        public DrawIndirectEntry(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;
        }

        public DrawIndirectEntry Init(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;

            return this;
        }

        public override void ClearReferences()
        {
            IndirectBuffer = null;
        }
    }
}