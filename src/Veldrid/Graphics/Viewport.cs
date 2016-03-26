namespace Veldrid.Graphics
{
    public struct Viewport
    {
        public readonly int X, Y, Width, Height;

        public Viewport(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
