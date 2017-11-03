namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetScissorRectEntry : OpenGLCommandEntry
    {
        public uint Index;
        public uint X;
        public uint Y;
        public uint Width;
        public uint Height;

        public SetScissorRectEntry(uint index, uint x, uint y, uint width, uint height)
        {
            Index = index;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public SetScissorRectEntry() { }

        public SetScissorRectEntry Init(uint index, uint x, uint y, uint width, uint height)
        {
            Index = index;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            return this;
        }

        public override void ClearReferences()
        {
        }
    }
}