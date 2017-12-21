namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class DrawIndexedIndirectEntry : OpenGLCommandEntry
    {
        public DeviceBuffer IndirectBuffer;
        public uint Offset;
        public uint DrawCount;
        public uint Stride;

        public DrawIndexedIndirectEntry() { }

        public DrawIndexedIndirectEntry(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;
        }

        public DrawIndexedIndirectEntry Init(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
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