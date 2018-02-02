using System;

namespace Veldrid
{
    internal static class FormatHelpers
    {
        public static uint GetSizeInBytes(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                    return 1;
                case PixelFormat.R8_G8_SNorm:
                case PixelFormat.R16_UNorm:
                    return 2;
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.R32_Float:
                    return 4;
                case PixelFormat.R32_G32_B32_A32_Float:
                case PixelFormat.R32_G32_B32_A32_UInt:
                    return 16;
                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC3_UNorm:
                    return 1; // Not really
                default: throw Illegal.Value<PixelFormat>();
            }
        }

        public static uint GetSizeInBytes(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Byte2_Norm:
                case VertexElementFormat.Byte2:
                case VertexElementFormat.SByte2_Norm:
                case VertexElementFormat.SByte2:
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
                    return 4;
                case VertexElementFormat.Float2:
                case VertexElementFormat.UInt2:
                case VertexElementFormat.Int2:
                case VertexElementFormat.UShort4_Norm:
                case VertexElementFormat.UShort4:
                case VertexElementFormat.Short4_Norm:
                case VertexElementFormat.Short4:
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

        public static int GetElementCount(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                case VertexElementFormat.UInt1:
                case VertexElementFormat.Int1:
                    return 1;
                case VertexElementFormat.Float2:
                case VertexElementFormat.Byte2_Norm:
                case VertexElementFormat.Byte2:
                case VertexElementFormat.SByte2_Norm:
                case VertexElementFormat.SByte2:
                case VertexElementFormat.UShort2_Norm:
                case VertexElementFormat.UShort2:
                case VertexElementFormat.Short2_Norm:
                case VertexElementFormat.Short2:
                case VertexElementFormat.UInt2:
                case VertexElementFormat.Int2:
                    return 2;
                case VertexElementFormat.Float3:
                case VertexElementFormat.UInt3:
                case VertexElementFormat.Int3:
                    return 3;
                case VertexElementFormat.Float4:
                case VertexElementFormat.Byte4_Norm:
                case VertexElementFormat.Byte4:
                case VertexElementFormat.SByte4_Norm:
                case VertexElementFormat.SByte4:
                case VertexElementFormat.UShort4_Norm:
                case VertexElementFormat.UShort4:
                case VertexElementFormat.Short4_Norm:
                case VertexElementFormat.Short4:
                case VertexElementFormat.UInt4:
                case VertexElementFormat.Int4:
                    return 4;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        internal static uint GetSampleCountUInt32(TextureSampleCount sampleCount)
        {
            switch (sampleCount)
            {
                case TextureSampleCount.Count1:
                    return 1;
                case TextureSampleCount.Count2:
                    return 2;
                case TextureSampleCount.Count4:
                    return 4;
                case TextureSampleCount.Count8:
                    return 8;
                case TextureSampleCount.Count16:
                    return 16;
                case TextureSampleCount.Count32:
                    return 32;
                default:
                    throw Illegal.Value<TextureSampleCount>();
            }
        }

        internal static bool IsStencilFormat(PixelFormat format)
        {
            return format == PixelFormat.D24_UNorm_S8_UInt || format == PixelFormat.D32_Float_S8_UInt;
        }

        internal static bool IsCompressedFormat(PixelFormat format)
        {
            return format == PixelFormat.BC1_Rgb_UNorm
                || format == PixelFormat.BC1_Rgba_UNorm
                || format == PixelFormat.BC2_UNorm
                || format == PixelFormat.BC3_UNorm;
        }

        internal static uint GetRowPitch(uint width, PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC3_UNorm:
                    var blocksPerRow = (width + 3) / 4;
                    var blockSizeInBytes = GetBlockSizeInBytes(format);
                    return blocksPerRow * blockSizeInBytes;

                default:
                    return width * GetSizeInBytes(format);
            }
        }

        public static uint GetBlockSizeInBytes(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgb_UNorm:
                    return 8;
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC3_UNorm:
                    return 16;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static uint GetNumRows(uint height, PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC3_UNorm:
                    return (height + 3) / 4;

                default:
                    return height;
            }
        }

        internal static uint GetDepthPitch(uint rowPitch, uint height, PixelFormat format)
        {
            return rowPitch * GetNumRows(height, format);
        }
    }
}
