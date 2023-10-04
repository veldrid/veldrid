using System.Diagnostics.CodeAnalysis;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal static class MTLFormats
    {
        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static MTLPixelFormat VdToMTLPixelFormat(PixelFormat format, bool depthFormat)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                    return MTLPixelFormat.R8Unorm;
                case PixelFormat.R8_SNorm:
                    return MTLPixelFormat.R8Snorm;
                case PixelFormat.R8_UInt:
                    return MTLPixelFormat.R8Uint;
                case PixelFormat.R8_SInt:
                    return MTLPixelFormat.R8Sint;

                case PixelFormat.R16_UNorm:
                    return depthFormat ? MTLPixelFormat.Depth16Unorm : MTLPixelFormat.R16Unorm;
                case PixelFormat.R16_SNorm:
                    return MTLPixelFormat.R16Snorm;
                case PixelFormat.R16_UInt:
                    return MTLPixelFormat.R16Uint;
                case PixelFormat.R16_SInt:
                    return MTLPixelFormat.R16Sint;
                case PixelFormat.R16_Float:
                    return MTLPixelFormat.R16Float;

                case PixelFormat.R32_UInt:
                    return MTLPixelFormat.R32Uint;
                case PixelFormat.R32_SInt:
                    return MTLPixelFormat.R32Sint;
                case PixelFormat.R32_Float:
                    return depthFormat ? MTLPixelFormat.Depth32Float : MTLPixelFormat.R32Float;

                case PixelFormat.R8_G8_UNorm:
                    return MTLPixelFormat.RG8Unorm;
                case PixelFormat.R8_G8_SNorm:
                    return MTLPixelFormat.RG8Snorm;
                case PixelFormat.R8_G8_UInt:
                    return MTLPixelFormat.RG8Uint;
                case PixelFormat.R8_G8_SInt:
                    return MTLPixelFormat.RG8Sint;

                case PixelFormat.R16_G16_UNorm:
                    return MTLPixelFormat.RG16Unorm;
                case PixelFormat.R16_G16_SNorm:
                    return MTLPixelFormat.RG16Snorm;
                case PixelFormat.R16_G16_UInt:
                    return MTLPixelFormat.RG16Uint;
                case PixelFormat.R16_G16_SInt:
                    return MTLPixelFormat.RG16Sint;
                case PixelFormat.R16_G16_Float:
                    return MTLPixelFormat.RG16Float;

                case PixelFormat.R32_G32_UInt:
                    return MTLPixelFormat.RG32Uint;
                case PixelFormat.R32_G32_SInt:
                    return MTLPixelFormat.RG32Sint;
                case PixelFormat.R32_G32_Float:
                    return MTLPixelFormat.RG32Float;

                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return MTLPixelFormat.RGBA8Unorm;
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                    return MTLPixelFormat.RGBA8Unorm_sRGB;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return MTLPixelFormat.BGRA8Unorm;
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return MTLPixelFormat.BGRA8Unorm_sRGB;
                case PixelFormat.R8_G8_B8_A8_SNorm:
                    return MTLPixelFormat.RGBA8Snorm;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return MTLPixelFormat.RGBA8Uint;
                case PixelFormat.R8_G8_B8_A8_SInt:
                    return MTLPixelFormat.RGBA8Sint;

                case PixelFormat.R16_G16_B16_A16_UNorm:
                    return MTLPixelFormat.RGBA16Unorm;
                case PixelFormat.R16_G16_B16_A16_SNorm:
                    return MTLPixelFormat.RGBA16Snorm;
                case PixelFormat.R16_G16_B16_A16_UInt:
                    return MTLPixelFormat.RGBA16Uint;
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return MTLPixelFormat.RGBA16Sint;
                case PixelFormat.R16_G16_B16_A16_Float:
                    return MTLPixelFormat.RGBA16Float;

                case PixelFormat.R32_G32_B32_A32_UInt:
                    return MTLPixelFormat.RGBA32Uint;
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return MTLPixelFormat.RGBA32Sint;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return MTLPixelFormat.RGBA32Float;

                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgba_UNorm:
                    return MTLPixelFormat.BC1_RGBA;
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                    return MTLPixelFormat.BC1_RGBA_sRGB;
                case PixelFormat.BC2_UNorm:
                    return MTLPixelFormat.BC2_RGBA;
                case PixelFormat.BC2_UNorm_SRgb:
                    return MTLPixelFormat.BC2_RGBA_sRGB;
                case PixelFormat.BC3_UNorm:
                    return MTLPixelFormat.BC3_RGBA;
                case PixelFormat.BC3_UNorm_SRgb:
                    return MTLPixelFormat.BC3_RGBA_sRGB;
                case PixelFormat.BC4_UNorm:
                    return MTLPixelFormat.BC4_RUnorm;
                case PixelFormat.BC4_SNorm:
                    return MTLPixelFormat.BC4_RSnorm;
                case PixelFormat.BC5_UNorm:
                    return MTLPixelFormat.BC5_RGUnorm;
                case PixelFormat.BC5_SNorm:
                    return MTLPixelFormat.BC5_RGSnorm;
                case PixelFormat.BC7_UNorm:
                    return MTLPixelFormat.BC7_RGBAUnorm;
                case PixelFormat.BC7_UNorm_SRgb:
                    return MTLPixelFormat.BC7_RGBAUnorm_sRGB;

                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                    return MTLPixelFormat.ETC2_RGB8;
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                    return MTLPixelFormat.ETC2_RGB8A1;
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return MTLPixelFormat.EAC_RGBA8;

                case PixelFormat.D24_UNorm_S8_UInt:
                    return MTLPixelFormat.Depth24Unorm_Stencil8;
                case PixelFormat.D32_Float_S8_UInt:
                    return MTLPixelFormat.Depth32Float_Stencil8;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return MTLPixelFormat.RGB10A2Unorm;
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return MTLPixelFormat.RGB10A2Uint;
                case PixelFormat.R11_G11_B10_Float:
                    return MTLPixelFormat.RG11B10Float;

                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static bool IsFormatSupported(PixelFormat format, TextureUsage usage, MTLFeatureSupport metalFeatures)
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
                    return metalFeatures.IsSupported(MTLFeatureSet.macOS_GPUFamily1_v1)
                        || metalFeatures.IsSupported(MTLFeatureSet.macOS_GPUFamily1_v2)
                        || metalFeatures.IsSupported(MTLFeatureSet.macOS_GPUFamily1_v3);

                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return metalFeatures.IsSupported(MTLFeatureSet.iOS_GPUFamily1_v1)
                        || metalFeatures.IsSupported(MTLFeatureSet.iOS_GPUFamily2_v1)
                        || metalFeatures.IsSupported(MTLFeatureSet.iOS_GPUFamily3_v1)
                        || metalFeatures.IsSupported(MTLFeatureSet.iOS_GPUFamily4_v1);

                case PixelFormat.R16_UNorm:
                    return ((usage & TextureUsage.DepthStencil) == 0)
                        || metalFeatures.IsSupported(MTLFeatureSet.macOS_GPUFamily1_v2)
                        || metalFeatures.IsSupported(MTLFeatureSet.macOS_GPUFamily1_v3);

                default:
                    return true;
            }
        }

        internal static MTLTriangleFillMode VdToMTLFillMode(PolygonFillMode fillMode)
        {
            return fillMode switch
            {
                PolygonFillMode.Solid => MTLTriangleFillMode.Fill,
                PolygonFillMode.Wireframe => MTLTriangleFillMode.Lines,
                _ => throw Illegal.Value<PolygonFillMode>(),
            };
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

        internal static MTLBlendFactor VdToMTLBlendFactor(BlendFactor vdFactor)
        {
            return vdFactor switch
            {
                BlendFactor.Zero => MTLBlendFactor.Zero,
                BlendFactor.One => MTLBlendFactor.One,
                BlendFactor.SourceAlpha => MTLBlendFactor.SourceAlpha,
                BlendFactor.InverseSourceAlpha => MTLBlendFactor.OneMinusSourceAlpha,
                BlendFactor.DestinationAlpha => MTLBlendFactor.DestinationAlpha,
                BlendFactor.InverseDestinationAlpha => MTLBlendFactor.OneMinusDestinationAlpha,
                BlendFactor.SourceColor => MTLBlendFactor.SourceColor,
                BlendFactor.InverseSourceColor => MTLBlendFactor.OneMinusSourceColor,
                BlendFactor.DestinationColor => MTLBlendFactor.DestinationColor,
                BlendFactor.InverseDestinationColor => MTLBlendFactor.OneMinusDestinationColor,
                BlendFactor.BlendFactor => MTLBlendFactor.BlendColor,
                BlendFactor.InverseBlendFactor => MTLBlendFactor.OneMinusBlendColor,
                _ => throw Illegal.Value<BlendFactor>(),
            };
        }

        internal static MTLBlendOperation VdToMTLBlendOp(BlendFunction vdFunction)
        {
            return vdFunction switch
            {
                BlendFunction.Add => MTLBlendOperation.Add,
                BlendFunction.Maximum => MTLBlendOperation.Max,
                BlendFunction.Minimum => MTLBlendOperation.Min,
                BlendFunction.ReverseSubtract => MTLBlendOperation.ReverseSubtract,
                BlendFunction.Subtract => MTLBlendOperation.Subtract,
                _ => throw Illegal.Value<BlendFunction>(),
            };
        }

        internal static MTLColorWriteMask VdToMTLColorWriteMask(ColorWriteMask vdMask)
        {
            MTLColorWriteMask mask = MTLColorWriteMask.None;

            if ((vdMask & ColorWriteMask.Red) == ColorWriteMask.Red)
                mask |= MTLColorWriteMask.Red;
            if ((vdMask & ColorWriteMask.Green) == ColorWriteMask.Green)
                mask |= MTLColorWriteMask.Green;
            if ((vdMask & ColorWriteMask.Blue) == ColorWriteMask.Blue)
                mask |= MTLColorWriteMask.Blue;
            if ((vdMask & ColorWriteMask.Alpha) == ColorWriteMask.Alpha)
                mask |= MTLColorWriteMask.Alpha;

            return mask;
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static MTLDataType VdVoMTLShaderConstantType(ShaderConstantType type)
        {
            switch (type)
            {
                case ShaderConstantType.Bool:
                    return MTLDataType.Bool;
                case ShaderConstantType.UInt16:
                    return MTLDataType.UShort;
                case ShaderConstantType.Int16:
                    return MTLDataType.Short;
                case ShaderConstantType.UInt32:
                    return MTLDataType.UInt;
                case ShaderConstantType.Int32:
                    return MTLDataType.Int;
                case ShaderConstantType.Float:
                    return MTLDataType.Float;
                case ShaderConstantType.UInt64:
                case ShaderConstantType.Int64:
                case ShaderConstantType.Double:
                    throw new VeldridException($"Metal does not support 64-bit shader constants.");
                default:
                    throw Illegal.Value<ShaderConstantType>();
            }
        }

        internal static MTLCompareFunction VdToMTLCompareFunction(ComparisonKind comparisonKind)
        {
            return comparisonKind switch
            {
                ComparisonKind.Always => MTLCompareFunction.Always,
                ComparisonKind.Equal => MTLCompareFunction.Equal,
                ComparisonKind.Greater => MTLCompareFunction.Greater,
                ComparisonKind.GreaterEqual => MTLCompareFunction.GreaterEqual,
                ComparisonKind.Less => MTLCompareFunction.Less,
                ComparisonKind.LessEqual => MTLCompareFunction.LessEqual,
                ComparisonKind.Never => MTLCompareFunction.Never,
                ComparisonKind.NotEqual => MTLCompareFunction.NotEqual,
                _ => throw Illegal.Value<ComparisonKind>(),
            };
        }

        internal static MTLCullMode VdToMTLCullMode(FaceCullMode cullMode)
        {
            return cullMode switch
            {
                FaceCullMode.Front => MTLCullMode.Front,
                FaceCullMode.Back => MTLCullMode.Back,
                FaceCullMode.None => MTLCullMode.None,
                _ => throw Illegal.Value<FaceCullMode>(),
            };
        }

        internal static MTLSamplerBorderColor VdToMTLBorderColor(SamplerBorderColor borderColor)
        {
            return borderColor switch
            {
                SamplerBorderColor.TransparentBlack => MTLSamplerBorderColor.TransparentBlack,
                SamplerBorderColor.OpaqueBlack => MTLSamplerBorderColor.OpaqueBlack,
                SamplerBorderColor.OpaqueWhite => MTLSamplerBorderColor.OpaqueWhite,
                _ => throw Illegal.Value<SamplerBorderColor>(),
            };
        }

        internal static MTLSamplerAddressMode VdToMTLAddressMode(SamplerAddressMode mode)
        {
            return mode switch
            {
                SamplerAddressMode.Border => MTLSamplerAddressMode.ClampToBorderColor,
                SamplerAddressMode.Clamp => MTLSamplerAddressMode.ClampToEdge,
                SamplerAddressMode.Mirror => MTLSamplerAddressMode.MirrorRepeat,
                SamplerAddressMode.Wrap => MTLSamplerAddressMode.Repeat,
                _ => throw Illegal.Value<SamplerAddressMode>(),
            };
        }

        internal static MTLPrimitiveType VdToMTLPrimitiveTopology(PrimitiveTopology primitiveTopology)
        {
            return primitiveTopology switch
            {
                PrimitiveTopology.LineList => MTLPrimitiveType.Line,
                PrimitiveTopology.LineStrip => MTLPrimitiveType.LineStrip,
                PrimitiveTopology.TriangleList => MTLPrimitiveType.Triangle,
                PrimitiveTopology.TriangleStrip => MTLPrimitiveType.TriangleStrip,
                PrimitiveTopology.PointList => MTLPrimitiveType.Point,
                _ => throw Illegal.Value<PrimitiveTopology>(),
            };
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
            return format switch
            {
                VertexElementFormat.Byte2_Norm => MTLVertexFormat.uchar2Normalized,
                VertexElementFormat.Byte2 => MTLVertexFormat.uchar2,
                VertexElementFormat.Byte4_Norm => MTLVertexFormat.uchar4Normalized,
                VertexElementFormat.Byte4 => MTLVertexFormat.uchar4,
                VertexElementFormat.SByte2_Norm => MTLVertexFormat.char2Normalized,
                VertexElementFormat.SByte2 => MTLVertexFormat.char2,
                VertexElementFormat.SByte4_Norm => MTLVertexFormat.char4Normalized,
                VertexElementFormat.SByte4 => MTLVertexFormat.char4,
                VertexElementFormat.UShort2_Norm => MTLVertexFormat.ushort2Normalized,
                VertexElementFormat.UShort2 => MTLVertexFormat.ushort2,
                VertexElementFormat.Short2_Norm => MTLVertexFormat.short2Normalized,
                VertexElementFormat.Short2 => MTLVertexFormat.short2,
                VertexElementFormat.UShort4_Norm => MTLVertexFormat.ushort4Normalized,
                VertexElementFormat.UShort4 => MTLVertexFormat.ushort4,
                VertexElementFormat.Short4_Norm => MTLVertexFormat.short4Normalized,
                VertexElementFormat.Short4 => MTLVertexFormat.short4,
                VertexElementFormat.UInt1 => MTLVertexFormat.@uint,
                VertexElementFormat.UInt2 => MTLVertexFormat.uint2,
                VertexElementFormat.UInt3 => MTLVertexFormat.uint3,
                VertexElementFormat.UInt4 => MTLVertexFormat.uint4,
                VertexElementFormat.Int1 => MTLVertexFormat.@int,
                VertexElementFormat.Int2 => MTLVertexFormat.int2,
                VertexElementFormat.Int3 => MTLVertexFormat.int3,
                VertexElementFormat.Int4 => MTLVertexFormat.int4,
                VertexElementFormat.Float1 => MTLVertexFormat.@float,
                VertexElementFormat.Float2 => MTLVertexFormat.float2,
                VertexElementFormat.Float3 => MTLVertexFormat.float3,
                VertexElementFormat.Float4 => MTLVertexFormat.float4,
                VertexElementFormat.Half1 => MTLVertexFormat.half,
                VertexElementFormat.Half2 => MTLVertexFormat.half2,
                VertexElementFormat.Half4 => MTLVertexFormat.half4,
                _ => throw Illegal.Value<VertexElementFormat>(),
            };
        }

        internal static MTLIndexType VdToMTLIndexFormat(IndexFormat format)
        {
            return format == IndexFormat.UInt16 ? MTLIndexType.UInt16 : MTLIndexType.UInt32;
        }

        internal static MTLStencilOperation VdToMTLStencilOperation(StencilOperation op)
        {
            return op switch
            {
                StencilOperation.Keep => MTLStencilOperation.Keep,
                StencilOperation.Zero => MTLStencilOperation.Zero,
                StencilOperation.Replace => MTLStencilOperation.Replace,
                StencilOperation.IncrementAndClamp => MTLStencilOperation.IncrementClamp,
                StencilOperation.DecrementAndClamp => MTLStencilOperation.DecrementClamp,
                StencilOperation.Invert => MTLStencilOperation.Invert,
                StencilOperation.IncrementAndWrap => MTLStencilOperation.IncrementWrap,
                StencilOperation.DecrementAndWrap => MTLStencilOperation.DecrementWrap,
                _ => throw Illegal.Value<StencilOperation>(),
            };
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static uint GetMaxTexture1DWidth(MTLFeatureSet fs)
        {
            switch (fs)
            {
                case MTLFeatureSet.iOS_GPUFamily1_v1:
                case MTLFeatureSet.iOS_GPUFamily2_v1:
                    return 4096;
                case MTLFeatureSet.iOS_GPUFamily1_v2:
                case MTLFeatureSet.iOS_GPUFamily2_v2:
                case MTLFeatureSet.iOS_GPUFamily1_v3:
                case MTLFeatureSet.iOS_GPUFamily2_v3:
                case MTLFeatureSet.iOS_GPUFamily1_v4:
                case MTLFeatureSet.iOS_GPUFamily2_v4:
                case MTLFeatureSet.tvOS_GPUFamily1_v1:
                case MTLFeatureSet.tvOS_GPUFamily1_v2:
                case MTLFeatureSet.tvOS_GPUFamily1_v3:
                    return 8192;
                case MTLFeatureSet.iOS_GPUFamily3_v1:
                case MTLFeatureSet.iOS_GPUFamily3_v2:
                case MTLFeatureSet.iOS_GPUFamily3_v3:
                case MTLFeatureSet.iOS_GPUFamily4_v1:
                case MTLFeatureSet.tvOS_GPUFamily2_v1:
                case MTLFeatureSet.macOS_GPUFamily1_v1:
                case MTLFeatureSet.macOS_GPUFamily1_v2:
                case MTLFeatureSet.macOS_GPUFamily1_v3:
                    return 16384;
                default:
                    return 4096;
            }
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static uint GetMaxTexture2DDimensions(MTLFeatureSet fs)
        {
            switch (fs)
            {
                case MTLFeatureSet.iOS_GPUFamily1_v1:
                case MTLFeatureSet.iOS_GPUFamily2_v1:
                    return 4096;
                case MTLFeatureSet.iOS_GPUFamily1_v2:
                case MTLFeatureSet.iOS_GPUFamily2_v2:
                case MTLFeatureSet.iOS_GPUFamily1_v3:
                case MTLFeatureSet.iOS_GPUFamily2_v3:
                case MTLFeatureSet.iOS_GPUFamily1_v4:
                case MTLFeatureSet.iOS_GPUFamily2_v4:
                case MTLFeatureSet.tvOS_GPUFamily1_v1:
                case MTLFeatureSet.tvOS_GPUFamily1_v2:
                case MTLFeatureSet.tvOS_GPUFamily1_v3:
                    return 8192;
                case MTLFeatureSet.iOS_GPUFamily3_v1:
                case MTLFeatureSet.iOS_GPUFamily3_v2:
                case MTLFeatureSet.iOS_GPUFamily3_v3:
                case MTLFeatureSet.iOS_GPUFamily4_v1:
                case MTLFeatureSet.tvOS_GPUFamily2_v1:
                case MTLFeatureSet.macOS_GPUFamily1_v1:
                case MTLFeatureSet.macOS_GPUFamily1_v2:
                case MTLFeatureSet.macOS_GPUFamily1_v3:
                    return 16384;
                default:
                    return 4096;
            }
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static uint GetMaxTextureCubeDimensions(MTLFeatureSet fs)
        {
            switch (fs)
            {
                case MTLFeatureSet.iOS_GPUFamily1_v1:
                case MTLFeatureSet.iOS_GPUFamily2_v1:
                    return 4096;
                case MTLFeatureSet.iOS_GPUFamily1_v2:
                case MTLFeatureSet.iOS_GPUFamily2_v2:
                case MTLFeatureSet.iOS_GPUFamily1_v3:
                case MTLFeatureSet.iOS_GPUFamily2_v3:
                case MTLFeatureSet.iOS_GPUFamily1_v4:
                case MTLFeatureSet.iOS_GPUFamily2_v4:
                case MTLFeatureSet.tvOS_GPUFamily1_v1:
                case MTLFeatureSet.tvOS_GPUFamily1_v2:
                case MTLFeatureSet.tvOS_GPUFamily1_v3:
                    return 8192;
                case MTLFeatureSet.iOS_GPUFamily3_v1:
                case MTLFeatureSet.iOS_GPUFamily3_v2:
                case MTLFeatureSet.iOS_GPUFamily3_v3:
                case MTLFeatureSet.iOS_GPUFamily4_v1:
                case MTLFeatureSet.tvOS_GPUFamily2_v1:
                case MTLFeatureSet.macOS_GPUFamily1_v1:
                case MTLFeatureSet.macOS_GPUFamily1_v2:
                case MTLFeatureSet.macOS_GPUFamily1_v3:
                    return 16384;
                default:
                    return 4096;
            }
        }

        internal static uint GetMaxTextureVolume(MTLFeatureSet fs)
        {
            return 2048;
        }
    }
}
