using System;

namespace Veldrid.Graphics
{
    internal static class FormatHelpers
    {
        public static int GetIndexFormatElementByteSize(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt32:
                    return 4;
                case IndexFormat.UInt16:
                    return 2;
                case IndexFormat.UInt8:
                    return 1;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        public static float GetPixelSize(PixelFormat veldridFormat)
        {
            switch (veldridFormat)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return 16;
                case PixelFormat.R16_UInt:
                    return 2;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return 4;
                case PixelFormat.R8_UInt:
                    return 1;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }
    }
}
