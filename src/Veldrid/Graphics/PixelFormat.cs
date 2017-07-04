namespace Veldrid.Graphics
{
    public enum PixelFormat : byte
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
        /// A format with a single color channel represented by an 16-bit unsigned integer.
        /// </summary>
        R16_UInt,
        /// <summary>
        /// A format where each pixel has four components (RGBA), each represented by an 8-bit unsigned integer value.
        /// </summary>
        R8_G8_B8_A8_UInt,
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
                case PixelFormat.R8_UInt:
                    return sizeof(byte);
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return sizeof(byte) * 4;
                case PixelFormat.R16_UInt:
                    return sizeof(ushort);
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }
    }
}
