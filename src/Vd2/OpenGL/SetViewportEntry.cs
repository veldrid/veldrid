namespace Vd2.OpenGL
{
    internal class SetViewportEntry : OpenGLCommandEntry
    {
        public readonly uint Index;
        public readonly Viewport Viewport;

        public SetViewportEntry(uint index, ref Viewport viewport)
        {
            Index = index;
            Viewport = viewport;
        }
    }
}