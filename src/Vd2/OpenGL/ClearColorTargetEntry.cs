namespace Vd2.OpenGL
{
    internal class ClearColorTargetEntry : OpenGLCommandEntry
    {
        public readonly uint Index;
        public readonly RgbaFloat ClearColor;

        public ClearColorTargetEntry(uint index, RgbaFloat clearColor)
        {
            Index = index;
            ClearColor = clearColor;
        }
    }
}