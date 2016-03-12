namespace Veldrid.Graphics
{
    public enum PixelFormat
    {
        /// <summary>
        /// A format where each component (RGBA) is represented by a 32-bit floating point value.
        /// </summary>
        R32_G32_B32_A32_Float,
        /// <summary>
        /// A format where each pixel is represented by a single byte (0-255) greyscale value.
        /// </summary>
        Alpha_UInt8,
        /// <summary>
        /// A format where each pixel has four components (RGBA), each represented by an 8-bit unsigned integer value.
        /// </summary>
        R8_G8_B8_A8
    }
}
