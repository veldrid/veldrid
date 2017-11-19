namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class DrawIndirectEntry : OpenGLCommandEntry
    {
        public Buffer IndirectBuffer;
        public uint Offset;
        public uint DrawCount;
        public uint Stride;

        public DrawIndirectEntry() { }

        public DrawIndirectEntry(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;
        }

        public DrawIndirectEntry Init(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
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