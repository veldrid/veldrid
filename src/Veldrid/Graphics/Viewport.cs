namespace Veldrid.Graphics
{
    /// <summary>
    /// Describes the renderable area used by the rasterizer.
    /// </summary>
    public struct Viewport
    {
        /// <summary>
        /// The X coordinate of the top-left of the viewport area.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// The Y coordinate of the top-left of the viewport area.
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// The width of the viewport area.
        /// </summary>
        public readonly int Width;
        
        /// <summary>
        /// The height of the viewport area.
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Constructs a new <see cref="Viewport"/>.
        /// </summary>
        /// <param name="x">The X coordinate of the top-left of the viewport area.</param>
        /// <param name="y">The Y coordinate of the top-left of the viewport area.</param>
        /// <param name="width">The width of the viewport area.</param>
        /// <param name="height">The height of the viewport area.</param>
        public Viewport(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
