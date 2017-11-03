namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetViewportEntry : OpenGLCommandEntry
    {
        public uint Index;
        public Viewport Viewport;

        public SetViewportEntry(uint index, ref Viewport viewport)
        {
            Index = index;
            Viewport = viewport;
        }

        public SetViewportEntry() { }

        public SetViewportEntry Init(uint index, ref Viewport viewport)
        {
            Index = index;
            Viewport = viewport;
            return this;
        }

        public override void ClearReferences()
        {
        }
    }
}