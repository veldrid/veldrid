using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal static class MTLFormats
    {
        internal static MTLPixelFormat VdToMTLPixelFormat(PixelFormat format, bool depthFormat)
        {
            switch (format)
            {
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return MTLPixelFormat.BGRA8Unorm;
                case PixelFormat.BC3_UNorm:
                    return MTLPixelFormat.BC3_RGBA;
                case PixelFormat.D24_UNorm_S8_UInt:
                    return MTLPixelFormat.Depth24Unorm_Stencil8;
                case PixelFormat.D32_Float_S8_UInt:
                    return MTLPixelFormat.Depth32Float_Stencil8;
                case PixelFormat.R16_UNorm:
                    return depthFormat ? MTLPixelFormat.Depth16Unorm : MTLPixelFormat.R16Unorm;
                case PixelFormat.R32_Float:
                    return depthFormat ? MTLPixelFormat.Depth32Float : MTLPixelFormat.R32Float;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return MTLPixelFormat.RGBA32Float;
                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return MTLPixelFormat.RGBA8Unorm;
                case PixelFormat.R8_UNorm:
                    return MTLPixelFormat.R8Unorm;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static MTLWinding VdVoMTLFrontFace(FrontFace frontFace)
        {
            return frontFace == FrontFace.CounterClockwise ? MTLWinding.CounterClockwise : MTLWinding.Clockwise;
        }

        internal static void GetMinMagMipFilter(
            SamplerFilter filter,
            out MTLSamplerMinMagFilter min,
            out MTLSamplerMinMagFilter mag,
            out MTLSamplerMipFilter mip)
        {
            switch (filter)
            {
                case SamplerFilter.Anisotropic:
                    min = mag = MTLSamplerMinMagFilter.Linear;
                    mip = MTLSamplerMipFilter.Linear;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipLinear:
                    min = MTLSamplerMinMagFilter.Linear;
                    mag = MTLSamplerMinMagFilter.Linear;
                    mip = MTLSamplerMipFilter.Linear;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipPoint:
                    min = MTLSamplerMinMagFilter.Linear;
                    mag = MTLSamplerMinMagFilter.Linear;
                    mip = MTLSamplerMipFilter.Nearest;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipLinear:
                    min = MTLSamplerMinMagFilter.Linear;
                    mag = MTLSamplerMinMagFilter.Nearest;
                    mip = MTLSamplerMipFilter.Linear;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipPoint:
                    min = MTLSamplerMinMagFilter.Linear;
                    mag = MTLSamplerMinMagFilter.Nearest;
                    mip = MTLSamplerMipFilter.Nearest;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipLinear:
                    min = MTLSamplerMinMagFilter.Nearest;
                    mag = MTLSamplerMinMagFilter.Linear;
                    mip = MTLSamplerMipFilter.Linear;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipPoint:
                    min = MTLSamplerMinMagFilter.Nearest;
                    mag = MTLSamplerMinMagFilter.Linear;
                    mip = MTLSamplerMipFilter.Nearest;
                    break;
                case SamplerFilter.MinPoint_MagPoint_MipLinear:
                    min = MTLSamplerMinMagFilter.Nearest;
                    mag = MTLSamplerMinMagFilter.Nearest;
                    mip = MTLSamplerMipFilter.Nearest;
                    break;
                case SamplerFilter.MinPoint_MagPoint_MipPoint:
                    min = MTLSamplerMinMagFilter.Nearest;
                    mag = MTLSamplerMinMagFilter.Nearest;
                    mip = MTLSamplerMipFilter.Nearest;
                    break;
                default:
                    throw Illegal.Value<SamplerFilter>();
            }
        }

        internal static MTLTextureType VdToMTLTextureType(
            TextureType type,
            uint arrayLayers,
            bool multiSampled,
            bool cube)
        {
            switch (type)
            {
                case TextureType.Texture1D:
                    return arrayLayers > 1 ? MTLTextureType.Type1DArray : MTLTextureType.Type1D;
                case TextureType.Texture2D:
                    if (cube)
                    {
                        return arrayLayers > 1 ? MTLTextureType.TypeCubeArray : MTLTextureType.TypeCube;
                    }
                    else if (multiSampled)
                    {
                        return MTLTextureType.Type2DMultisample;
                    }
                    else
                    {
                        return arrayLayers > 1 ? MTLTextureType.Type2DArray : MTLTextureType.Type2D;
                    }
                case TextureType.Texture3D:
                    return MTLTextureType.Type3D;
                default:
                    throw Illegal.Value<TextureType>();
            }
        }

        internal static MTLCompareFunction VdToMTLCompareFunction(ComparisonKind comparisonKind)
        {
            switch (comparisonKind)
            {
                case ComparisonKind.Always:
                    return MTLCompareFunction.Always;
                case ComparisonKind.Equal:
                    return MTLCompareFunction.Equal;
                case ComparisonKind.Greater:
                    return MTLCompareFunction.Greater;
                case ComparisonKind.GreaterEqual:
                    return MTLCompareFunction.GreaterEqual;
                case ComparisonKind.Less:
                    return MTLCompareFunction.Less;
                case ComparisonKind.LessEqual:
                    return MTLCompareFunction.LessEqual;
                case ComparisonKind.Never:
                    return MTLCompareFunction.Never;
                case ComparisonKind.NotEqual:
                    return MTLCompareFunction.NotEqual;
                default:
                    throw Illegal.Value<ComparisonKind>();
            }
        }

        internal static MTLCullMode VdToMTLCullMode(FaceCullMode cullMode)
        {
            switch (cullMode)
            {
                case FaceCullMode.Front:
                    return MTLCullMode.Front;
                case FaceCullMode.Back:
                    return MTLCullMode.Back;
                case FaceCullMode.None:
                    return MTLCullMode.None;
                default:
                    throw Illegal.Value<FaceCullMode>();
            }
        }

        internal static MTLSamplerBorderColor VdToMTLBorderColor(SamplerBorderColor borderColor)
        {
            switch (borderColor)
            {
                case SamplerBorderColor.TransparentBlack:
                    return MTLSamplerBorderColor.TransparentBlack;
                case SamplerBorderColor.OpaqueBlack:
                    return MTLSamplerBorderColor.OpaqueBlack;
                case SamplerBorderColor.OpaqueWhite:
                    return MTLSamplerBorderColor.OpaqueWhite;
                default:
                    throw Illegal.Value<SamplerBorderColor>();
            }
        }

        internal static MTLSamplerAddressMode VdToMTLAddressMode(SamplerAddressMode mode)
        {
            switch (mode)
            {
                case SamplerAddressMode.Border:
                    return MTLSamplerAddressMode.ClampToBorderColor;
                case SamplerAddressMode.Clamp:
                    return MTLSamplerAddressMode.ClampToEdge;
                case SamplerAddressMode.Mirror:
                    return MTLSamplerAddressMode.MirrorClampToEdge;
                case SamplerAddressMode.Wrap:
                    return MTLSamplerAddressMode.Repeat;
                default:
                    throw Illegal.Value<SamplerAddressMode>();
            }
        }

        internal static MTLPrimitiveType VdToMTLPrimitiveTopology(PrimitiveTopology primitiveTopology)
        {
            switch (primitiveTopology)
            {
                case PrimitiveTopology.LineList:
                    return MTLPrimitiveType.Line;
                case PrimitiveTopology.LineStrip:
                    return MTLPrimitiveType.LineStrip;
                case PrimitiveTopology.TriangleList:
                    return MTLPrimitiveType.Triangle;
                case PrimitiveTopology.TriangleStrip:
                    return MTLPrimitiveType.TriangleStrip;
                default:
                    throw Illegal.Value<PrimitiveTopology>();
            }
        }

        internal static MTLTextureUsage VdToMTLTextureUsage(TextureUsage usage)
        {
            MTLTextureUsage ret = MTLTextureUsage.Unknown;

            if ((usage & TextureUsage.Sampled) == TextureUsage.Sampled)
            {
                ret |= MTLTextureUsage.ShaderRead;
            }
            if ((usage & TextureUsage.Storage) == TextureUsage.Storage)
            {
                ret |= MTLTextureUsage.ShaderWrite;
            }
            if ((usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil
                || (usage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget)
            {
                ret |= MTLTextureUsage.RenderTarget;
            }

            return ret;
        }

        internal static MTLVertexFormat VdToMTLVertexFormat(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Byte2_UNorm:
                    return MTLVertexFormat.uchar2Normalized;
                case VertexElementFormat.Byte2_UInt:
                    return MTLVertexFormat.uchar2;
                case VertexElementFormat.Byte4_UNorm:
                    return MTLVertexFormat.uchar4Normalized;
                case VertexElementFormat.Byte4_UInt:
                    return MTLVertexFormat.uchar4;
                case VertexElementFormat.Float1:
                    return MTLVertexFormat.@float;
                case VertexElementFormat.Float2:
                    return MTLVertexFormat.float2;
                case VertexElementFormat.Float3:
                    return MTLVertexFormat.float3;
                case VertexElementFormat.Float4:
                    return MTLVertexFormat.float4;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        internal static MTLIndexType VdToMTLIndexFormat(IndexFormat format)
        {
            return format == IndexFormat.UInt16 ? MTLIndexType.UInt16 : MTLIndexType.UInt32;
        }
    }
}