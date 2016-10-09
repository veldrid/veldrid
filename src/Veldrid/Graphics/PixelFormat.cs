namespace Veldrid.Graphics
{
    public enum PixelFormat
    {
        /// <summary>
        /// A format where each component (RGBA) is represented by a 32-bit floating point value.
        /// </summary>
        R32_G32_B32_A32_Float,
        /// <summary>
        /// A format with a single color channel represented by an 8-bit unsigned integer.
        /// </summary>
        R8_UInt,
        /// <summary>
        /// A format where each pixel is represented by a single byte (0-255) greyscale value.
        /// </summary>
        Alpha_UInt8,
        /// <summary>
        /// A format where each pixel has four components (RGBA), each represented by an 8-bit unsigned integer value.
        /// </summary>
        R8_G8_B8_A8,
        /// <summary>
        /// A format where each pixel is represented by a single UInt16 value.
        /// </summary>
        Alpha_UInt16
    }

    public static class PixelFormatEx
    {
        /// <summary>
        /// Gets the size of an individual pixel data point for the given <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="format">The format of pixel data.</param>
        /// <returns>The size of an individual pixel in bytes.</returns>
        public static int GetSizeInBytes(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return sizeof(float) * 4;
                case PixelFormat.Alpha_UInt8:
                    return sizeof(byte);
                case PixelFormat.R8_G8_B8_A8:
                    return sizeof(byte) * 4;
                case PixelFormat.Alpha_UInt16:
                    return sizeof(ushort);
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }
    }
}
