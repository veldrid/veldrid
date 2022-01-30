using System.Diagnostics.CodeAnalysis;
using Vulkan;

namespace Veldrid.Vk
{
    internal static partial class VkFormats
    {
        internal static VkSamplerAddressMode VdToVkSamplerAddressMode(SamplerAddressMode mode)
        {
            return mode switch
            {
                SamplerAddressMode.Wrap => VkSamplerAddressMode.Repeat,
                SamplerAddressMode.Mirror => VkSamplerAddressMode.MirroredRepeat,
                SamplerAddressMode.Clamp => VkSamplerAddressMode.ClampToEdge,
                SamplerAddressMode.Border => VkSamplerAddressMode.ClampToBorder,
                _ => throw Illegal.Value<SamplerAddressMode>(),
            };
        }

        internal static void GetFilterParams(
            SamplerFilter filter,
            out VkFilter minFilter,
            out VkFilter magFilter,
            out VkSamplerMipmapMode mipmapMode)
        {
            switch (filter)
            {
                case SamplerFilter.Anisotropic:
                    minFilter = VkFilter.Linear;
                    magFilter = VkFilter.Linear;
                    mipmapMode = VkSamplerMipmapMode.Linear;
                    break;
                case SamplerFilter.MinPoint_MagPoint_MipPoint:
                    minFilter = VkFilter.Nearest;
                    magFilter = VkFilter.Nearest;
                    mipmapMode = VkSamplerMipmapMode.Nearest;
                    break;
                case SamplerFilter.MinPoint_MagPoint_MipLinear:
                    minFilter = VkFilter.Nearest;
                    magFilter = VkFilter.Nearest;
                    mipmapMode = VkSamplerMipmapMode.Linear;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipPoint:
                    minFilter = VkFilter.Nearest;
                    magFilter = VkFilter.Linear;
                    mipmapMode = VkSamplerMipmapMode.Nearest;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipLinear:
                    minFilter = VkFilter.Nearest;
                    magFilter = VkFilter.Linear;
                    mipmapMode = VkSamplerMipmapMode.Linear;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipPoint:
                    minFilter = VkFilter.Linear;
                    magFilter = VkFilter.Nearest;
                    mipmapMode = VkSamplerMipmapMode.Nearest;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipLinear:
                    minFilter = VkFilter.Linear;
                    magFilter = VkFilter.Nearest;
                    mipmapMode = VkSamplerMipmapMode.Linear;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipPoint:
                    minFilter = VkFilter.Linear;
                    magFilter = VkFilter.Linear;
                    mipmapMode = VkSamplerMipmapMode.Nearest;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipLinear:
                    minFilter = VkFilter.Linear;
                    magFilter = VkFilter.Linear;
                    mipmapMode = VkSamplerMipmapMode.Linear;
                    break;
                default:
                    throw Illegal.Value<SamplerFilter>();
            }
        }

        internal static VkImageUsageFlags VdToVkTextureUsage(TextureUsage vdUsage)
        {
            VkImageUsageFlags vkUsage = VkImageUsageFlags.TransferDst | VkImageUsageFlags.TransferSrc;
            bool isDepthStencil = (vdUsage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil;
            if ((vdUsage & TextureUsage.Sampled) == TextureUsage.Sampled)
            {
                vkUsage |= VkImageUsageFlags.Sampled;
            }
            if (isDepthStencil)
            {
                vkUsage |= VkImageUsageFlags.DepthStencilAttachment;
            }
            if ((vdUsage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget)
            {
                vkUsage |= VkImageUsageFlags.ColorAttachment;
            }
            if ((vdUsage & TextureUsage.Storage) == TextureUsage.Storage)
            {
                vkUsage |= VkImageUsageFlags.Storage;
            }

            return vkUsage;
        }

        internal static VkImageType VdToVkTextureType(TextureType type)
        {
            return type switch
            {
                TextureType.Texture1D => VkImageType.Image1D,
                TextureType.Texture2D => VkImageType.Image2D,
                TextureType.Texture3D => VkImageType.Image3D,
                _ => throw Illegal.Value<TextureType>(),
            };
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static VkDescriptorType VdToVkDescriptorType(ResourceKind kind, ResourceLayoutElementOptions options)
        {
            bool dynamicBinding = (options & ResourceLayoutElementOptions.DynamicBinding) != 0;
            switch (kind)
            {
                case ResourceKind.UniformBuffer:
                    return dynamicBinding ? VkDescriptorType.UniformBufferDynamic : VkDescriptorType.UniformBuffer;
                case ResourceKind.StructuredBufferReadWrite:
                case ResourceKind.StructuredBufferReadOnly:
                    return dynamicBinding ? VkDescriptorType.StorageBufferDynamic : VkDescriptorType.StorageBuffer;
                case ResourceKind.TextureReadOnly:
                    return VkDescriptorType.SampledImage;
                case ResourceKind.TextureReadWrite:
                    return VkDescriptorType.StorageImage;
                case ResourceKind.Sampler:
                    return VkDescriptorType.Sampler;
                default:
                    throw Illegal.Value<ResourceKind>();
            }
        }

        internal static VkSampleCountFlags VdToVkSampleCount(TextureSampleCount sampleCount)
        {
            return sampleCount switch
            {
                TextureSampleCount.Count1 => VkSampleCountFlags.Count1,
                TextureSampleCount.Count2 => VkSampleCountFlags.Count2,
                TextureSampleCount.Count4 => VkSampleCountFlags.Count4,
                TextureSampleCount.Count8 => VkSampleCountFlags.Count8,
                TextureSampleCount.Count16 => VkSampleCountFlags.Count16,
                TextureSampleCount.Count32 => VkSampleCountFlags.Count32,
                _ => throw Illegal.Value<TextureSampleCount>(),
            };
        }

        internal static VkStencilOp VdToVkStencilOp(StencilOperation op)
        {
            return op switch
            {
                StencilOperation.Keep => VkStencilOp.Keep,
                StencilOperation.Zero => VkStencilOp.Zero,
                StencilOperation.Replace => VkStencilOp.Replace,
                StencilOperation.IncrementAndClamp => VkStencilOp.IncrementAndClamp,
                StencilOperation.DecrementAndClamp => VkStencilOp.DecrementAndClamp,
                StencilOperation.Invert => VkStencilOp.Invert,
                StencilOperation.IncrementAndWrap => VkStencilOp.IncrementAndWrap,
                StencilOperation.DecrementAndWrap => VkStencilOp.DecrementAndWrap,
                _ => throw Illegal.Value<StencilOperation>(),
            };
        }

        internal static VkPolygonMode VdToVkPolygonMode(PolygonFillMode fillMode)
        {
            return fillMode switch
            {
                PolygonFillMode.Solid => VkPolygonMode.Fill,
                PolygonFillMode.Wireframe => VkPolygonMode.Line,
                _ => throw Illegal.Value<PolygonFillMode>(),
            };
        }

        internal static VkCullModeFlags VdToVkCullMode(FaceCullMode cullMode)
        {
            return cullMode switch
            {
                FaceCullMode.Back => VkCullModeFlags.Back,
                FaceCullMode.Front => VkCullModeFlags.Front,
                FaceCullMode.None => VkCullModeFlags.None,
                _ => throw Illegal.Value<FaceCullMode>(),
            };
        }

        internal static VkBlendOp VdToVkBlendOp(BlendFunction func)
        {
            return func switch
            {
                BlendFunction.Add => VkBlendOp.Add,
                BlendFunction.Subtract => VkBlendOp.Subtract,
                BlendFunction.ReverseSubtract => VkBlendOp.ReverseSubtract,
                BlendFunction.Minimum => VkBlendOp.Min,
                BlendFunction.Maximum => VkBlendOp.Max,
                _ => throw Illegal.Value<BlendFunction>(),
            };
        }

        internal static VkPrimitiveTopology VdToVkPrimitiveTopology(PrimitiveTopology topology)
        {
            return topology switch
            {
                PrimitiveTopology.TriangleList => VkPrimitiveTopology.TriangleList,
                PrimitiveTopology.TriangleStrip => VkPrimitiveTopology.TriangleStrip,
                PrimitiveTopology.LineList => VkPrimitiveTopology.LineList,
                PrimitiveTopology.LineStrip => VkPrimitiveTopology.LineStrip,
                PrimitiveTopology.PointList => VkPrimitiveTopology.PointList,
                _ => throw Illegal.Value<PrimitiveTopology>(),
            };
        }

        internal static uint GetSpecializationConstantSize(ShaderConstantType type)
        {
            return type switch
            {
                ShaderConstantType.Bool => 4,
                ShaderConstantType.UInt16 => 2,
                ShaderConstantType.Int16 => 2,
                ShaderConstantType.UInt32 => 4,
                ShaderConstantType.Int32 => 4,
                ShaderConstantType.UInt64 => 8,
                ShaderConstantType.Int64 => 8,
                ShaderConstantType.Float => 4,
                ShaderConstantType.Double => 8,
                _ => throw Illegal.Value<ShaderConstantType>(),
            };
        }

        internal static VkBlendFactor VdToVkBlendFactor(BlendFactor factor)
        {
            return factor switch
            {
                BlendFactor.Zero => VkBlendFactor.Zero,
                BlendFactor.One => VkBlendFactor.One,
                BlendFactor.SourceAlpha => VkBlendFactor.SrcAlpha,
                BlendFactor.InverseSourceAlpha => VkBlendFactor.OneMinusSrcAlpha,
                BlendFactor.DestinationAlpha => VkBlendFactor.DstAlpha,
                BlendFactor.InverseDestinationAlpha => VkBlendFactor.OneMinusDstAlpha,
                BlendFactor.SourceColor => VkBlendFactor.SrcColor,
                BlendFactor.InverseSourceColor => VkBlendFactor.OneMinusSrcColor,
                BlendFactor.DestinationColor => VkBlendFactor.DstColor,
                BlendFactor.InverseDestinationColor => VkBlendFactor.OneMinusDstColor,
                BlendFactor.BlendFactor => VkBlendFactor.ConstantColor,
                BlendFactor.InverseBlendFactor => VkBlendFactor.OneMinusConstantColor,
                _ => throw Illegal.Value<BlendFactor>(),
            };
        }

        internal static VkFormat VdToVkVertexElementFormat(VertexElementFormat format)
        {
            return format switch
            {
                VertexElementFormat.Float1 => VkFormat.R32Sfloat,
                VertexElementFormat.Float2 => VkFormat.R32g32Sfloat,
                VertexElementFormat.Float3 => VkFormat.R32g32b32Sfloat,
                VertexElementFormat.Float4 => VkFormat.R32g32b32a32Sfloat,
                VertexElementFormat.Byte2_Norm => VkFormat.R8g8Unorm,
                VertexElementFormat.Byte2 => VkFormat.R8g8Uint,
                VertexElementFormat.Byte4_Norm => VkFormat.R8g8b8a8Unorm,
                VertexElementFormat.Byte4 => VkFormat.R8g8b8a8Uint,
                VertexElementFormat.SByte2_Norm => VkFormat.R8g8Snorm,
                VertexElementFormat.SByte2 => VkFormat.R8g8Sint,
                VertexElementFormat.SByte4_Norm => VkFormat.R8g8b8a8Snorm,
                VertexElementFormat.SByte4 => VkFormat.R8g8b8a8Sint,
                VertexElementFormat.UShort2_Norm => VkFormat.R16g16Unorm,
                VertexElementFormat.UShort2 => VkFormat.R16g16Uint,
                VertexElementFormat.UShort4_Norm => VkFormat.R16g16b16a16Unorm,
                VertexElementFormat.UShort4 => VkFormat.R16g16b16a16Uint,
                VertexElementFormat.Short2_Norm => VkFormat.R16g16Snorm,
                VertexElementFormat.Short2 => VkFormat.R16g16Sint,
                VertexElementFormat.Short4_Norm => VkFormat.R16g16b16a16Snorm,
                VertexElementFormat.Short4 => VkFormat.R16g16b16a16Sint,
                VertexElementFormat.UInt1 => VkFormat.R32Uint,
                VertexElementFormat.UInt2 => VkFormat.R32g32Uint,
                VertexElementFormat.UInt3 => VkFormat.R32g32b32Uint,
                VertexElementFormat.UInt4 => VkFormat.R32g32b32a32Uint,
                VertexElementFormat.Int1 => VkFormat.R32Sint,
                VertexElementFormat.Int2 => VkFormat.R32g32Sint,
                VertexElementFormat.Int3 => VkFormat.R32g32b32Sint,
                VertexElementFormat.Int4 => VkFormat.R32g32b32a32Sint,
                VertexElementFormat.Half1 => VkFormat.R16Sfloat,
                VertexElementFormat.Half2 => VkFormat.R16g16Sfloat,
                VertexElementFormat.Half4 => VkFormat.R16g16b16a16Sfloat,
                _ => throw Illegal.Value<VertexElementFormat>(),
            };
        }

        internal static VkShaderStageFlags VdToVkShaderStages(ShaderStages stage)
        {
            VkShaderStageFlags ret = VkShaderStageFlags.None;

            if ((stage & ShaderStages.Vertex) == ShaderStages.Vertex)
                ret |= VkShaderStageFlags.Vertex;

            if ((stage & ShaderStages.Geometry) == ShaderStages.Geometry)
                ret |= VkShaderStageFlags.Geometry;

            if ((stage & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
                ret |= VkShaderStageFlags.TessellationControl;

            if ((stage & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
                ret |= VkShaderStageFlags.TessellationEvaluation;

            if ((stage & ShaderStages.Fragment) == ShaderStages.Fragment)
                ret |= VkShaderStageFlags.Fragment;

            if ((stage & ShaderStages.Compute) == ShaderStages.Compute)
                ret |= VkShaderStageFlags.Compute;

            return ret;
        }

        internal static VkBorderColor VdToVkSamplerBorderColor(SamplerBorderColor borderColor)
        {
            return borderColor switch
            {
                SamplerBorderColor.TransparentBlack => VkBorderColor.FloatTransparentBlack,
                SamplerBorderColor.OpaqueBlack => VkBorderColor.FloatOpaqueBlack,
                SamplerBorderColor.OpaqueWhite => VkBorderColor.FloatOpaqueWhite,
                _ => throw Illegal.Value<SamplerBorderColor>(),
            };
        }

        internal static VkIndexType VdToVkIndexFormat(IndexFormat format)
        {
            return format switch
            {
                IndexFormat.UInt16 => VkIndexType.Uint16,
                IndexFormat.UInt32 => VkIndexType.Uint32,
                _ => throw Illegal.Value<IndexFormat>(),
            };
        }

        internal static VkCompareOp VdToVkCompareOp(ComparisonKind comparisonKind)
        {
            return comparisonKind switch
            {
                ComparisonKind.Never => VkCompareOp.Never,
                ComparisonKind.Less => VkCompareOp.Less,
                ComparisonKind.Equal => VkCompareOp.Equal,
                ComparisonKind.LessEqual => VkCompareOp.LessOrEqual,
                ComparisonKind.Greater => VkCompareOp.Greater,
                ComparisonKind.NotEqual => VkCompareOp.NotEqual,
                ComparisonKind.GreaterEqual => VkCompareOp.GreaterOrEqual,
                ComparisonKind.Always => VkCompareOp.Always,
                _ => throw Illegal.Value<ComparisonKind>(),
            };
        }

        internal static PixelFormat VkToVdPixelFormat(VkFormat vkFormat)
        {
            return vkFormat switch
            {
                VkFormat.R8Unorm => PixelFormat.R8_UNorm,
                VkFormat.R8Snorm => PixelFormat.R8_SNorm,
                VkFormat.R8Uint => PixelFormat.R8_UInt,
                VkFormat.R8Sint => PixelFormat.R8_SInt,
                VkFormat.R16Unorm => PixelFormat.R16_UNorm,
                VkFormat.R16Snorm => PixelFormat.R16_SNorm,
                VkFormat.R16Uint => PixelFormat.R16_UInt,
                VkFormat.R16Sint => PixelFormat.R16_SInt,
                VkFormat.R16Sfloat => PixelFormat.R16_Float,
                VkFormat.R32Uint => PixelFormat.R32_UInt,
                VkFormat.R32Sint => PixelFormat.R32_SInt,
                VkFormat.R32Sfloat or VkFormat.D32Sfloat => PixelFormat.R32_Float,
                VkFormat.R8g8Unorm => PixelFormat.R8_G8_UNorm,
                VkFormat.R8g8Snorm => PixelFormat.R8_G8_SNorm,
                VkFormat.R8g8Uint => PixelFormat.R8_G8_UInt,
                VkFormat.R8g8Sint => PixelFormat.R8_G8_SInt,
                VkFormat.R16g16Unorm => PixelFormat.R16_G16_UNorm,
                VkFormat.R16g16Snorm => PixelFormat.R16_G16_SNorm,
                VkFormat.R16g16Uint => PixelFormat.R16_G16_UInt,
                VkFormat.R16g16Sint => PixelFormat.R16_G16_SInt,
                VkFormat.R16g16Sfloat => PixelFormat.R16_G16_Float,
                VkFormat.R32g32Uint => PixelFormat.R32_G32_UInt,
                VkFormat.R32g32Sint => PixelFormat.R32_G32_SInt,
                VkFormat.R32g32Sfloat => PixelFormat.R32_G32_Float,
                VkFormat.R8g8b8a8Unorm => PixelFormat.R8_G8_B8_A8_UNorm,
                VkFormat.R8g8b8a8Srgb => PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                VkFormat.B8g8r8a8Unorm => PixelFormat.B8_G8_R8_A8_UNorm,
                VkFormat.B8g8r8a8Srgb => PixelFormat.B8_G8_R8_A8_UNorm_SRgb,
                VkFormat.R8g8b8a8Snorm => PixelFormat.R8_G8_B8_A8_SNorm,
                VkFormat.R8g8b8a8Uint => PixelFormat.R8_G8_B8_A8_UInt,
                VkFormat.R8g8b8a8Sint => PixelFormat.R8_G8_B8_A8_SInt,
                VkFormat.R16g16b16a16Unorm => PixelFormat.R16_G16_B16_A16_UNorm,
                VkFormat.R16g16b16a16Snorm => PixelFormat.R16_G16_B16_A16_SNorm,
                VkFormat.R16g16b16a16Uint => PixelFormat.R16_G16_B16_A16_UInt,
                VkFormat.R16g16b16a16Sint => PixelFormat.R16_G16_B16_A16_SInt,
                VkFormat.R16g16b16a16Sfloat => PixelFormat.R16_G16_B16_A16_Float,
                VkFormat.R32g32b32a32Uint => PixelFormat.R32_G32_B32_A32_UInt,
                VkFormat.R32g32b32a32Sint => PixelFormat.R32_G32_B32_A32_SInt,
                VkFormat.R32g32b32a32Sfloat => PixelFormat.R32_G32_B32_A32_Float,
                VkFormat.Bc1RgbUnormBlock => PixelFormat.BC1_Rgb_UNorm,
                VkFormat.Bc1RgbSrgbBlock => PixelFormat.BC1_Rgb_UNorm_SRgb,
                VkFormat.Bc1RgbaUnormBlock => PixelFormat.BC1_Rgba_UNorm,
                VkFormat.Bc1RgbaSrgbBlock => PixelFormat.BC1_Rgba_UNorm_SRgb,
                VkFormat.Bc2UnormBlock => PixelFormat.BC2_UNorm,
                VkFormat.Bc2SrgbBlock => PixelFormat.BC2_UNorm_SRgb,
                VkFormat.Bc3UnormBlock => PixelFormat.BC3_UNorm,
                VkFormat.Bc3SrgbBlock => PixelFormat.BC3_UNorm_SRgb,
                VkFormat.Bc4UnormBlock => PixelFormat.BC4_UNorm,
                VkFormat.Bc4SnormBlock => PixelFormat.BC4_SNorm,
                VkFormat.Bc5UnormBlock => PixelFormat.BC5_UNorm,
                VkFormat.Bc5SnormBlock => PixelFormat.BC5_SNorm,
                VkFormat.Bc7UnormBlock => PixelFormat.BC7_UNorm,
                VkFormat.Bc7SrgbBlock => PixelFormat.BC7_UNorm_SRgb,
                VkFormat.A2b10g10r10UnormPack32 => PixelFormat.R10_G10_B10_A2_UNorm,
                VkFormat.A2b10g10r10UintPack32 => PixelFormat.R10_G10_B10_A2_UInt,
                VkFormat.B10g11r11UfloatPack32 => PixelFormat.R11_G11_B10_Float,
                _ => throw Illegal.Value<VkFormat>(),
            };
        }
    }
}
