using System;

namespace Veldrid
{
    public static class FormatHelpers
    {
        public static uint GetSizeInBytes(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                    return 1;
                case PixelFormat.R16_UNorm:
                    return 2;
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.R32_Float:
                    return 4;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return 16;
                default: throw Illegal.Value<PixelFormat>();
            }
        }

        internal static uint GetSizeInBytes(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                case VertexElementFormat.Byte4:
                    return 4;
                case VertexElementFormat.Float2:
                    return 8;
                case VertexElementFormat.Float3:
                    return 12;
                case VertexElementFormat.Float4:
                    return 16;
                case VertexElementFormat.Byte1:
                    return 1;
                case VertexElementFormat.Byte2:
                    return 2;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        internal static int GetElementCount(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                case VertexElementFormat.Byte1:
                    return 1;
                case VertexElementFormat.Float2:
                case VertexElementFormat.Byte2:
                    return 2;
                case VertexElementFormat.Float3:
                    return 3;
                case VertexElementFormat.Float4:
                case VertexElementFormat.Byte4:
                    return 4;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }
    }
}
