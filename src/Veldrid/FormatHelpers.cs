using System;
using System.Diagnostics;

namespace Veldrid
{
    internal static class FormatHelpers
    {
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

        public static int GetElementCount(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                case VertexElementFormat.UInt1:
                case VertexElementFormat.Int1:
                case VertexElementFormat.Half1:
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
                case VertexElementFormat.Half2:
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
                case VertexElementFormat.Half4:
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

        internal static bool IsDepthStencilFormat(PixelFormat format)
        {
            return format == PixelFormat.D32_Float_S8_UInt
                || format == PixelFormat.D24_UNorm_S8_UInt
                || format == PixelFormat.R16_UNorm
                || format == PixelFormat.R32_Float;
        }

        internal static bool IsCompressedFormat(PixelFormat format)
        {
            return format == PixelFormat.BC1_Rgb_UNorm
                || format == PixelFormat.BC1_Rgb_UNorm_SRgb
                || format == PixelFormat.BC1_Rgba_UNorm
                || format == PixelFormat.BC1_Rgba_UNorm_SRgb
                || format == PixelFormat.BC2_UNorm
                || format == PixelFormat.BC2_UNorm_SRgb
                || format == PixelFormat.BC3_UNorm
                || format == PixelFormat.BC3_UNorm_SRgb
                || format == PixelFormat.BC4_UNorm
                || format == PixelFormat.BC4_SNorm
                || format == PixelFormat.BC5_UNorm
                || format == PixelFormat.BC5_SNorm
                || format == PixelFormat.BC7_UNorm
                || format == PixelFormat.BC7_UNorm_SRgb
                || format == PixelFormat.ETC2_R8_G8_B8_UNorm
                || format == PixelFormat.ETC2_R8_G8_B8_A1_UNorm
                || format == PixelFormat.ETC2_R8_G8_B8_A8_UNorm;
        }

        internal static uint GetRowPitch(uint width, PixelFormat format)
        {
            switch (format)
            {
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
                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC4_SNorm:
                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                    return 8;
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC5_SNorm:
                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return 16;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static bool IsFormatViewCompatible(PixelFormat viewFormat, PixelFormat realFormat)
        {
            if (IsCompressedFormat(realFormat))
            {
                return IsSrgbCounterpart(viewFormat, realFormat);
            }
            else
            {
                return GetViewFamilyFormat(viewFormat) == GetViewFamilyFormat(realFormat);
            }
        }

        private static bool IsSrgbCounterpart(PixelFormat viewFormat, PixelFormat realFormat)
        {
            throw new NotImplementedException();
        }

        internal static uint GetNumRows(uint height, PixelFormat format)
        {
            switch (format)
            {
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
                    return (height + 3) / 4;

                default:
                    return height;
            }
        }

        internal static uint GetDepthPitch(uint rowPitch, uint height, PixelFormat format)
        {
            return rowPitch * GetNumRows(height, format);
        }

        internal static uint GetRegionSize(uint width, uint height, uint depth, PixelFormat format)
        {
            uint blockSizeInBytes;
            if (IsCompressedFormat(format))
            {
                Debug.Assert((width % 4 == 0 || width < 4) && (height % 4 == 0 || height < 4));
                blockSizeInBytes = GetBlockSizeInBytes(format);
                width /= 4;
                height /= 4;
            }
            else
            {
                blockSizeInBytes = GetSizeInBytes(format);
            }

            return width * height * depth * blockSizeInBytes;
        }

        internal static TextureSampleCount GetSampleCount(uint samples)
        {
            switch (samples)
            {
                case 1: return TextureSampleCount.Count1;
                case 2: return TextureSampleCount.Count2;
                case 4: return TextureSampleCount.Count4;
                case 8: return TextureSampleCount.Count8;
                case 16: return TextureSampleCount.Count16;
                case 32: return TextureSampleCount.Count32;
                default: throw new VeldridException("Unsupported multisample count: " + samples);
            }
        }

        internal static PixelFormat GetViewFamilyFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                case PixelFormat.R32_G32_B32_A32_UInt:
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return PixelFormat.R32_G32_B32_A32_Float;
                case PixelFormat.R16_G16_B16_A16_Float:
                case PixelFormat.R16_G16_B16_A16_UNorm:
                case PixelFormat.R16_G16_B16_A16_UInt:
                case PixelFormat.R16_G16_B16_A16_SNorm:
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return PixelFormat.R16_G16_B16_A16_Float;
                case PixelFormat.R32_G32_Float:
                case PixelFormat.R32_G32_UInt:
                case PixelFormat.R32_G32_SInt:
                    return PixelFormat.R32_G32_Float;
                case PixelFormat.R10_G10_B10_A2_UNorm:
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return PixelFormat.R10_G10_B10_A2_UNorm;
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                case PixelFormat.R8_G8_B8_A8_UInt:
                case PixelFormat.R8_G8_B8_A8_SNorm:
                case PixelFormat.R8_G8_B8_A8_SInt:
                    return PixelFormat.R8_G8_B8_A8_UNorm;
                case PixelFormat.R16_G16_Float:
                case PixelFormat.R16_G16_UNorm:
                case PixelFormat.R16_G16_UInt:
                case PixelFormat.R16_G16_SNorm:
                case PixelFormat.R16_G16_SInt:
                    return PixelFormat.R16_G16_Float;
                case PixelFormat.R32_Float:
                case PixelFormat.R32_UInt:
                case PixelFormat.R32_SInt:
                    return PixelFormat.R32_Float;
                case PixelFormat.R8_G8_UNorm:
                case PixelFormat.R8_G8_UInt:
                case PixelFormat.R8_G8_SNorm:
                case PixelFormat.R8_G8_SInt:
                    return PixelFormat.R8_G8_UNorm;
                case PixelFormat.R16_Float:
                case PixelFormat.R16_UNorm:
                case PixelFormat.R16_UInt:
                case PixelFormat.R16_SNorm:
                case PixelFormat.R16_SInt:
                    return PixelFormat.R16_Float;
                case PixelFormat.R8_UNorm:
                case PixelFormat.R8_UInt:
                case PixelFormat.R8_SNorm:
                case PixelFormat.R8_SInt:
                    return PixelFormat.R8_UNorm;
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                    return PixelFormat.BC1_Rgba_UNorm;
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                    return PixelFormat.BC2_UNorm;
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                    return PixelFormat.BC3_UNorm;
                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC4_SNorm:
                    return PixelFormat.BC4_UNorm;
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC5_SNorm:
                    return PixelFormat.BC5_UNorm;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return PixelFormat.B8_G8_R8_A8_UNorm;
                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                    return PixelFormat.BC7_UNorm;
                default:
                    return format;
            }
        }
    }
}
