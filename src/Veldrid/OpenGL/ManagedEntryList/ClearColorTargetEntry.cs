namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class ClearColorTargetEntry : OpenGLCommandEntry
    {
        public uint Index;
        public RgbaFloat ClearColor;

        public ClearColorTargetEntry(uint index, RgbaFloat clearColor)
        {
            Index = index;
            ClearColor = clearColor;
        }

        public ClearColorTargetEntry() { }

        public ClearColorTargetEntry Init(uint index, RgbaFloat clearColor)
        {
            Index = index;
            ClearColor = clearColor;
            return this;
        }

        public override void ClearReferences()
        {
        }
    }
}