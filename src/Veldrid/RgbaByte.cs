namespace Veldrid
{
    /// <summary>
    /// A color stored in four 8-bit unsigned normalized integer values, in RGBA component order.
    /// </summary>
    public struct RgbaByte
    {
        /// <summary>
        /// The red component.
        /// </summary>
        public readonly byte R;
        /// <summary>
        /// The green component.
        /// </summary>
        public readonly byte G;
        /// <summary>
        /// The blue component.
        /// </summary>
        public readonly byte B;
        /// <summary>
        /// The alpha component.
        /// </summary>
        public readonly byte A;

        /// <summary>
        /// Black (0, 0, 0, 255)
        /// </summary>
        public static readonly RgbaByte Black = new RgbaByte(0, 0, 0, 255);
        /// <summary>
        /// White (255, 255, 255, 255)
        /// </summary>
        public static readonly RgbaByte White = new RgbaByte(255, 255, 255, 255);
        /// <summary>
        /// Cyan (0, 255, 255, 255)
        /// </summary>
        public static readonly RgbaByte Cyan = new RgbaByte(0, 255, 255, 255);
        /// <summary>
        /// Pink (255, 155, 191, 255)
        /// </summary>
        public static readonly RgbaByte Pink = new RgbaByte(255, 155, 191, 255);

        /// <summary>
        /// Constructs a new RgbaByte from the given components.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        public RgbaByte(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Returns a string representation of this color.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("R:{0}, G:{1}, B:{2}, A:{3}", R, G, B, A);
        }
    }
}
