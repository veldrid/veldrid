using System;
using System.Collections.Generic;
using Vulkan;

namespace Veldrid.Vk
{
    internal static partial class VkFormats
    {
        internal static VkSamplerAddressMode VdToVkSamplerAddressMode(SamplerAddressMode mode)
        {
            switch (mode)
            {
                case SamplerAddressMode.Wrap:
                    return VkSamplerAddressMode.Repeat;
                case SamplerAddressMode.Mirror:
                    return VkSamplerAddressMode.MirroredRepeat;
                case SamplerAddressMode.Clamp:
                    return VkSamplerAddressMode.ClampToEdge;
                case SamplerAddressMode.Border:
                    return VkSamplerAddressMode.ClampToBorder;
                default:
                    throw Illegal.Value<SamplerAddressMode>();
            }
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
            VkImageUsageFlags vkUsage = VkImageUsageFlags.None;

            vkUsage = VkImageUsageFlags.TransferDst | VkImageUsageFlags.TransferSrc;
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
            switch (type)
            {
                case TextureType.Texture1D:
                    return VkImageType.Image1D;
                case TextureType.Texture2D:
                    return VkImageType.Image2D;
                case TextureType.Texture3D:
                    return VkImageType.Image3D;
                default:
                    throw Illegal.Value<TextureType>();
            }
        }

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
            switch (sampleCount)
            {
                case TextureSampleCount.Count1:
                    return VkSampleCountFlags.Count1;
                case TextureSampleCount.Count2:
                    return VkSampleCountFlags.Count2;
                case TextureSampleCount.Count4:
                    return VkSampleCountFlags.Count4;
                case TextureSampleCount.Count8:
                    return VkSampleCountFlags.Count8;
                case TextureSampleCount.Count16:
                    return VkSampleCountFlags.Count16;
                case TextureSampleCount.Count32:
                    return VkSampleCountFlags.Count32;
                default:
                    throw Illegal.Value<TextureSampleCount>();
            }
        }

        internal static VkStencilOp VdToVkStencilOp(StencilOperation op)
        {
            switch (op)
            {
                case StencilOperation.Keep:
                    return VkStencilOp.Keep;
                case StencilOperation.Zero:
                    return VkStencilOp.Zero;
                case StencilOperation.Replace:
                    return VkStencilOp.Replace;
                case StencilOperation.IncrementAndClamp:
                    return VkStencilOp.IncrementAndClamp;
                case StencilOperation.DecrementAndClamp:
                    return VkStencilOp.DecrementAndClamp;
                case StencilOperation.Invert:
                    return VkStencilOp.Invert;
                case StencilOperation.IncrementAndWrap:
                    return VkStencilOp.IncrementAndWrap;
                case StencilOperation.DecrementAndWrap:
                    return VkStencilOp.DecrementAndWrap;
                default:
                    throw Illegal.Value<StencilOperation>();
            }
        }

        internal static VkPolygonMode VdToVkPolygonMode(PolygonFillMode fillMode)
        {
            switch (fillMode)
            {
                case PolygonFillMode.Solid:
                    return VkPolygonMode.Fill;
                case PolygonFillMode.Wireframe:
                    return VkPolygonMode.Line;
                default:
                    throw Illegal.Value<PolygonFillMode>();
            }
        }

        internal static VkCullModeFlags VdToVkCullMode(FaceCullMode cullMode)
        {
            switch (cullMode)
            {
                case FaceCullMode.Back:
                    return VkCullModeFlags.Back;
                case FaceCullMode.Front:
                    return VkCullModeFlags.Front;
                case FaceCullMode.None:
                    return VkCullModeFlags.None;
                default:
                    throw Illegal.Value<FaceCullMode>();
            }
        }

        internal static VkBlendOp VdToVkBlendOp(BlendFunction func)
        {
            switch (func)
            {
                case BlendFunction.Add:
                    return VkBlendOp.Add;
                case BlendFunction.Subtract:
                    return VkBlendOp.Subtract;
                case BlendFunction.ReverseSubtract:
                    return VkBlendOp.ReverseSubtract;
                case BlendFunction.Minimum:
                    return VkBlendOp.Min;
                case BlendFunction.Maximum:
                    return VkBlendOp.Max;
                default:
                    throw Illegal.Value<BlendFunction>();
            }
        }

        internal static VkPrimitiveTopology VdToVkPrimitiveTopology(PrimitiveTopology topology)
        {
            switch (topology)
            {
                case PrimitiveTopology.TriangleList:
                    return VkPrimitiveTopology.TriangleList;
                case PrimitiveTopology.TriangleStrip:
                    return VkPrimitiveTopology.TriangleStrip;
                case PrimitiveTopology.LineList:
                    return VkPrimitiveTopology.LineList;
                case PrimitiveTopology.LineStrip:
                    return VkPrimitiveTopology.LineStrip;
                case PrimitiveTopology.PointList:
                    return VkPrimitiveTopology.PointList;
                default:
                    throw Illegal.Value<PrimitiveTopology>();
            }
        }

        internal static uint GetSpecializationConstantSize(ShaderConstantType type)
        {
            switch (type)
            {
                case ShaderConstantType.Bool:
                    return 4;
                case ShaderConstantType.UInt16:
                    return 2;
                case ShaderConstantType.Int16:
                    return 2;
                case ShaderConstantType.UInt32:
                    return 4;
                case ShaderConstantType.Int32:
                    return 4;
                case ShaderConstantType.UInt64:
                    return 8;
                case ShaderConstantType.Int64:
                    return 8;
                case ShaderConstantType.Float:
                    return 4;
                case ShaderConstantType.Double:
                    return 8;
                default:
                    throw Illegal.Value<ShaderConstantType>();
            }
        }

        internal static VkBlendFactor VdToVkBlendFactor(BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Zero:
                    return VkBlendFactor.Zero;
                case BlendFactor.One:
                    return VkBlendFactor.One;
                case BlendFactor.SourceAlpha:
                    return VkBlendFactor.SrcAlpha;
                case BlendFactor.InverseSourceAlpha:
                    return VkBlendFactor.OneMinusSrcAlpha;
                case BlendFactor.DestinationAlpha:
                    return VkBlendFactor.DstAlpha;
                case BlendFactor.InverseDestinationAlpha:
                    return VkBlendFactor.OneMinusDstAlpha;
                case BlendFactor.SourceColor:
                    return VkBlendFactor.SrcColor;
                case BlendFactor.InverseSourceColor:
                    return VkBlendFactor.OneMinusSrcColor;
                case BlendFactor.DestinationColor:
                    return VkBlendFactor.DstColor;
                case BlendFactor.InverseDestinationColor:
                    return VkBlendFactor.OneMinusDstColor;
                case BlendFactor.BlendFactor:
                    return VkBlendFactor.ConstantColor;
                case BlendFactor.InverseBlendFactor:
                    return VkBlendFactor.OneMinusConstantColor;
                default:
                    throw Illegal.Value<BlendFactor>();
            }
        }

        internal static VkFormat VdToVkVertexElementFormat(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                    return VkFormat.R32Sfloat;
                case VertexElementFormat.Float2:
                    return VkFormat.R32g32Sfloat;
                case VertexElementFormat.Float3:
                    return VkFormat.R32g32b32Sfloat;
                case VertexElementFormat.Float4:
                    return VkFormat.R32g32b32a32Sfloat;
                case VertexElementFormat.Byte2_Norm:
                    return VkFormat.R8g8Unorm;
                case VertexElementFormat.Byte2:
                    return VkFormat.R8g8Uint;
                case VertexElementFormat.Byte4_Norm:
                    return VkFormat.R8g8b8a8Unorm;
                case VertexElementFormat.Byte4:
                    return VkFormat.R8g8b8a8Uint;
                case VertexElementFormat.SByte2_Norm:
                    return VkFormat.R8g8Snorm;
                case VertexElementFormat.SByte2:
                    return VkFormat.R8g8Sint;
                case VertexElementFormat.SByte4_Norm:
                    return VkFormat.R8g8b8a8Snorm;
                case VertexElementFormat.SByte4:
                    return VkFormat.R8g8b8a8Sint;
                case VertexElementFormat.UShort2_Norm:
                    return VkFormat.R16g16Unorm;
                case VertexElementFormat.UShort2:
                    return VkFormat.R16g16Uint;
                case VertexElementFormat.UShort4_Norm:
                    return VkFormat.R16g16b16a16Unorm;
                case VertexElementFormat.UShort4:
                    return VkFormat.R16g16b16a16Uint;
                case VertexElementFormat.Short2_Norm:
                    return VkFormat.R16g16Snorm;
                case VertexElementFormat.Short2:
                    return VkFormat.R16g16Sint;
                case VertexElementFormat.Short4_Norm:
                    return VkFormat.R16g16b16a16Snorm;
                case VertexElementFormat.Short4:
                    return VkFormat.R16g16b16a16Sint;
                case VertexElementFormat.UInt1:
                    return VkFormat.R32Uint;
                case VertexElementFormat.UInt2:
                    return VkFormat.R32g32Uint;
                case VertexElementFormat.UInt3:
                    return VkFormat.R32g32b32Uint;
                case VertexElementFormat.UInt4:
                    return VkFormat.R32g32b32a32Uint;
                case VertexElementFormat.Int1:
                    return VkFormat.R32Sint;
                case VertexElementFormat.Int2:
                    return VkFormat.R32g32Sint;
                case VertexElementFormat.Int3:
                    return VkFormat.R32g32b32Sint;
                case VertexElementFormat.Int4:
                    return VkFormat.R32g32b32a32Sint;
                case VertexElementFormat.Half1:
                    return VkFormat.R16Sfloat;
                case VertexElementFormat.Half2:
                    return VkFormat.R16g16Sfloat;
                case VertexElementFormat.Half4:
                    return VkFormat.R16g16b16a16Sfloat;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
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
            switch (borderColor)
            {
                case SamplerBorderColor.TransparentBlack:
                    return VkBorderColor.FloatTransparentBlack;
                case SamplerBorderColor.OpaqueBlack:
                    return VkBorderColor.FloatOpaqueBlack;
                case SamplerBorderColor.OpaqueWhite:
                    return VkBorderColor.FloatOpaqueWhite;
                default:
                    throw Illegal.Value<SamplerBorderColor>();
            }
        }

        internal static VkIndexType VdToVkIndexFormat(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt16:
                    return VkIndexType.Uint16;
                case IndexFormat.UInt32:
                    return VkIndexType.Uint32;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        internal static VkCompareOp VdToVkCompareOp(ComparisonKind comparisonKind)
        {
            switch (comparisonKind)
            {
                case ComparisonKind.Never:
                    return VkCompareOp.Never;
                case ComparisonKind.Less:
                    return VkCompareOp.Less;
                case ComparisonKind.Equal:
                    return VkCompareOp.Equal;
                case ComparisonKind.LessEqual:
                    return VkCompareOp.LessOrEqual;
                case ComparisonKind.Greater:
                    return VkCompareOp.Greater;
                case ComparisonKind.NotEqual:
                    return VkCompareOp.NotEqual;
                case ComparisonKind.GreaterEqual:
                    return VkCompareOp.GreaterOrEqual;
                case ComparisonKind.Always:
                    return VkCompareOp.Always;
                default:
                    throw Illegal.Value<ComparisonKind>();
            }
        }

        internal static PixelFormat VkToVdPixelFormat(VkFormat vkFormat)
        {
            switch (vkFormat)
            {
                case VkFormat.R8Unorm:
                    return PixelFormat.R8_UNorm;
                case VkFormat.R8Snorm:
                    return PixelFormat.R8_SNorm;
                case VkFormat.R8Uint:
                    return PixelFormat.R8_UInt;
                case VkFormat.R8Sint:
                    return PixelFormat.R8_SInt;

                case VkFormat.R16Unorm:
                    return PixelFormat.R16_UNorm;
                case VkFormat.R16Snorm:
                    return PixelFormat.R16_SNorm;
                case VkFormat.R16Uint:
                    return PixelFormat.R16_UInt;
                case VkFormat.R16Sint:
                    return PixelFormat.R16_SInt;
                case VkFormat.R16Sfloat:
                    return PixelFormat.R16_Float;

                case VkFormat.R32Uint:
                    return PixelFormat.R32_UInt;
                case VkFormat.R32Sint:
                    return PixelFormat.R32_SInt;
                case VkFormat.R32Sfloat:
                case VkFormat.D32Sfloat:
                    return PixelFormat.R32_Float;

                case VkFormat.R8g8Unorm:
                    return PixelFormat.R8_G8_UNorm;
                case VkFormat.R8g8Snorm:
                    return PixelFormat.R8_G8_SNorm;
                case VkFormat.R8g8Uint:
                    return PixelFormat.R8_G8_UInt;
                case VkFormat.R8g8Sint:
                    return PixelFormat.R8_G8_SInt;

                case VkFormat.R16g16Unorm:
                    return PixelFormat.R16_G16_UNorm;
                case VkFormat.R16g16Snorm:
                    return PixelFormat.R16_G16_SNorm;
                case VkFormat.R16g16Uint:
                    return PixelFormat.R16_G16_UInt;
                case VkFormat.R16g16Sint:
                    return PixelFormat.R16_G16_SInt;
                case VkFormat.R16g16Sfloat:
                    return PixelFormat.R16_G16_Float;

                case VkFormat.R32g32Uint:
                    return PixelFormat.R32_G32_UInt;
                case VkFormat.R32g32Sint:
                    return PixelFormat.R32_G32_SInt;
                case VkFormat.R32g32Sfloat:
                    return PixelFormat.R32_G32_Float;

                case VkFormat.R8g8b8a8Unorm:
                    return PixelFormat.R8_G8_B8_A8_UNorm;
                case VkFormat.R8g8b8a8Srgb:
                    return PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
                case VkFormat.B8g8r8a8Unorm:
                    return PixelFormat.B8_G8_R8_A8_UNorm;
                case VkFormat.B8g8r8a8Srgb:
                    return PixelFormat.B8_G8_R8_A8_UNorm_SRgb;
                case VkFormat.R8g8b8a8Snorm:
                    return PixelFormat.R8_G8_B8_A8_SNorm;
                case VkFormat.R8g8b8a8Uint:
                    return PixelFormat.R8_G8_B8_A8_UInt;
                case VkFormat.R8g8b8a8Sint:
                    return PixelFormat.R8_G8_B8_A8_SInt;

                case VkFormat.R16g16b16a16Unorm:
                    return PixelFormat.R16_G16_B16_A16_UNorm;
                case VkFormat.R16g16b16a16Snorm:
                    return PixelFormat.R16_G16_B16_A16_SNorm;
                case VkFormat.R16g16b16a16Uint:
                    return PixelFormat.R16_G16_B16_A16_UInt;
                case VkFormat.R16g16b16a16Sint:
                    return PixelFormat.R16_G16_B16_A16_SInt;
                case VkFormat.R16g16b16a16Sfloat:
                    return PixelFormat.R16_G16_B16_A16_Float;

                case VkFormat.R32g32b32a32Uint:
                    return PixelFormat.R32_G32_B32_A32_UInt;
                case VkFormat.R32g32b32a32Sint:
                    return PixelFormat.R32_G32_B32_A32_SInt;
                case VkFormat.R32g32b32a32Sfloat:
                    return PixelFormat.R32_G32_B32_A32_Float;

                case VkFormat.Bc1RgbUnormBlock:
                    return PixelFormat.BC1_Rgb_UNorm;
                case VkFormat.Bc1RgbSrgbBlock:
                    return PixelFormat.BC1_Rgb_UNorm_SRgb;
                case VkFormat.Bc1RgbaUnormBlock:
                    return PixelFormat.BC1_Rgba_UNorm;
                case VkFormat.Bc1RgbaSrgbBlock:
                    return PixelFormat.BC1_Rgba_UNorm_SRgb;
                case VkFormat.Bc2UnormBlock:
                    return PixelFormat.BC2_UNorm;
                case VkFormat.Bc2SrgbBlock:
                    return PixelFormat.BC2_UNorm_SRgb;
                case VkFormat.Bc3UnormBlock:
                    return PixelFormat.BC3_UNorm;
                case VkFormat.Bc3SrgbBlock:
                    return PixelFormat.BC3_UNorm_SRgb;
                case VkFormat.Bc4UnormBlock:
                    return PixelFormat.BC4_UNorm;
                case VkFormat.Bc4SnormBlock:
                    return PixelFormat.BC4_SNorm;
                case VkFormat.Bc5UnormBlock:
                    return PixelFormat.BC5_UNorm;
                case VkFormat.Bc5SnormBlock:
                    return PixelFormat.BC5_SNorm;
                case VkFormat.Bc7UnormBlock:
                    return PixelFormat.BC7_UNorm;
                case VkFormat.Bc7SrgbBlock:
                    return PixelFormat.BC7_UNorm_SRgb;

                case VkFormat.A2b10g10r10UnormPack32:
                    return PixelFormat.R10_G10_B10_A2_UNorm;
                case VkFormat.A2b10g10r10UintPack32:
                    return PixelFormat.R10_G10_B10_A2_UInt;
                case VkFormat.B10g11r11UfloatPack32:
                    return PixelFormat.R11_G11_B10_Float;

                default:
                    throw Illegal.Value<VkFormat>();
            }
        }
    }
}
