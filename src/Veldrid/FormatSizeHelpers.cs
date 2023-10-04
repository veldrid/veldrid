using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Veldrid
{
    public static class FormatSizeHelpers
    {
        /// <summary>
        /// Given a pixel format, returns the number of bytes required to store a single pixel.
        /// </summary>
        /// <param name="format">An uncompressed pixel format.</param>
        /// <returns>The number of bytes required to store a single pixel in the given format.</returns>
        public static uint GetSizeInBytes(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                case PixelFormat.R8_SNorm:
                case PixelFormat.R8_UInt:
                case PixelFormat.R8_SInt:
                    return 1;

                case PixelFormat.R16_UNorm:
                case PixelFormat.R16_SNorm:
                case PixelFormat.R16_UInt:
                case PixelFormat.R16_SInt:
                case PixelFormat.R16_Float:
                case PixelFormat.R8_G8_UNorm:
                case PixelFormat.R8_G8_SNorm:
                case PixelFormat.R8_G8_UInt:
                case PixelFormat.R8_G8_SInt:
                    return 2;

                case PixelFormat.R32_UInt:
                case PixelFormat.R32_SInt:
                case PixelFormat.R32_Float:
                case PixelFormat.R16_G16_UNorm:
                case PixelFormat.R16_G16_SNorm:
                case PixelFormat.R16_G16_UInt:
                case PixelFormat.R16_G16_SInt:
                case PixelFormat.R16_G16_Float:
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                case PixelFormat.R8_G8_B8_A8_SNorm:
                case PixelFormat.R8_G8_B8_A8_UInt:
                case PixelFormat.R8_G8_B8_A8_SInt:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                case PixelFormat.R10_G10_B10_A2_UNorm:
                case PixelFormat.R10_G10_B10_A2_UInt:
                case PixelFormat.R11_G11_B10_Float:
                case PixelFormat.D24_UNorm_S8_UInt:
                    return 4;

                case PixelFormat.D32_Float_S8_UInt:
                    return 5;

                case PixelFormat.R16_G16_B16_A16_UNorm:
                case PixelFormat.R16_G16_B16_A16_SNorm:
                case PixelFormat.R16_G16_B16_A16_UInt:
                case PixelFormat.R16_G16_B16_A16_SInt:
                case PixelFormat.R16_G16_B16_A16_Float:
                case PixelFormat.R32_G32_UInt:
                case PixelFormat.R32_G32_SInt:
                case PixelFormat.R32_G32_Float:
                    return 8;

                case PixelFormat.R32_G32_B32_A32_Float:
                case PixelFormat.R32_G32_B32_A32_UInt:
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return 16;

                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC4_SNorm:
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC5_SNorm:
                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    Debug.Fail("GetSizeInBytes should not be used on a compressed format.");
                    throw Illegal.Value<PixelFormat>();

                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        /// <summary>
        /// Given a vertex element format, returns the number of bytes required
        /// to store an element in that format.
        /// </summary>
        /// <param name="format">A vertex element format.</param>
        /// <returns>The number of bytes required to store an element in the given format.</returns>
        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        public static uint GetSizeInBytes(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Byte2_Norm:
                case VertexElementFormat.Byte2:
                case VertexElementFormat.SByte2_Norm:
                case VertexElementFormat.SByte2:
                case VertexElementFormat.Half1:
                    return 2;
                case VertexElementFormat.Float1:
                case VertexElementFormat.UInt1:
                case VertexElementFormat.Int1:
                case VertexElementFormat.Byte4_Norm:
                case VertexElementFormat.Byte4:
                case VertexElementFormat.SByte4_Norm:
                case VertexElementFormat.SByte4:
                case VertexElementFormat.UShort2_Norm:
                case VertexElementFormat.UShort2:
                case VertexElementFormat.Short2_Norm:
                case VertexElementFormat.Short2:
                case VertexElementFormat.Half2:
                    return 4;
                case VertexElementFormat.Float2:
                case VertexElementFormat.UInt2:
                case VertexElementFormat.Int2:
                case VertexElementFormat.UShort4_Norm:
                case VertexElementFormat.UShort4:
                case VertexElementFormat.Short4_Norm:
                case VertexElementFormat.Short4:
                case VertexElementFormat.Half4:
                    return 8;
                case VertexElementFormat.Float3:
                case VertexElementFormat.UInt3:
                case VertexElementFormat.Int3:
                    return 12;
                case VertexElementFormat.Float4:
                case VertexElementFormat.UInt4:
                case VertexElementFormat.Int4:
                    return 16;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }
    }
}
