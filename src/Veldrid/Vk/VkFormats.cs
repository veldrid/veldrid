using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.VkBlendFactor;
using static TerraFX.Interop.Vulkan.VkBlendOp;
using static TerraFX.Interop.Vulkan.VkBorderColor;
using static TerraFX.Interop.Vulkan.VkCompareOp;
using static TerraFX.Interop.Vulkan.VkCullModeFlags;
using static TerraFX.Interop.Vulkan.VkDescriptorType;
using static TerraFX.Interop.Vulkan.VkFilter;
using static TerraFX.Interop.Vulkan.VkFormat;
using static TerraFX.Interop.Vulkan.VkImageType;
using static TerraFX.Interop.Vulkan.VkImageUsageFlags;
using static TerraFX.Interop.Vulkan.VkIndexType;
using static TerraFX.Interop.Vulkan.VkPolygonMode;
using static TerraFX.Interop.Vulkan.VkPrimitiveTopology;
using static TerraFX.Interop.Vulkan.VkSampleCountFlags;
using static TerraFX.Interop.Vulkan.VkSamplerAddressMode;
using static TerraFX.Interop.Vulkan.VkSamplerMipmapMode;
using static TerraFX.Interop.Vulkan.VkShaderStageFlags;
using static TerraFX.Interop.Vulkan.VkStencilOp;

namespace Veldrid.Vulkan
{
    internal static partial class VkFormats
    {
        internal static VkSamplerAddressMode VdToVkSamplerAddressMode(SamplerAddressMode mode)
        {
            return mode switch
            {
                SamplerAddressMode.Wrap => VK_SAMPLER_ADDRESS_MODE_REPEAT,
                SamplerAddressMode.Mirror => VK_SAMPLER_ADDRESS_MODE_MIRRORED_REPEAT,
                SamplerAddressMode.Clamp => VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_EDGE,
                SamplerAddressMode.Border => VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_BORDER,
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
                    minFilter = VK_FILTER_LINEAR;
                    magFilter = VK_FILTER_LINEAR;
                    mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
                    break;
                case SamplerFilter.MinPoint_MagPoint_MipPoint:
                    minFilter = VK_FILTER_NEAREST;
                    magFilter = VK_FILTER_NEAREST;
                    mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
                    break;
                case SamplerFilter.MinPoint_MagPoint_MipLinear:
                    minFilter = VK_FILTER_NEAREST;
                    magFilter = VK_FILTER_NEAREST;
                    mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipPoint:
                    minFilter = VK_FILTER_NEAREST;
                    magFilter = VK_FILTER_LINEAR;
                    mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipLinear:
                    minFilter = VK_FILTER_NEAREST;
                    magFilter = VK_FILTER_LINEAR;
                    mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipPoint:
                    minFilter = VK_FILTER_LINEAR;
                    magFilter = VK_FILTER_NEAREST;
                    mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipLinear:
                    minFilter = VK_FILTER_LINEAR;
                    magFilter = VK_FILTER_NEAREST;
                    mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipPoint:
                    minFilter = VK_FILTER_LINEAR;
                    magFilter = VK_FILTER_LINEAR;
                    mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipLinear:
                    minFilter = VK_FILTER_LINEAR;
                    magFilter = VK_FILTER_LINEAR;
                    mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
                    break;
                default:
                    throw Illegal.Value<SamplerFilter>();
            }
        }

        internal static VkImageUsageFlags VdToVkTextureUsage(TextureUsage vdUsage)
        {
            VkImageUsageFlags vkUsage = VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
            bool isDepthStencil = (vdUsage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil;
            if ((vdUsage & TextureUsage.Sampled) == TextureUsage.Sampled)
            {
                vkUsage |= VK_IMAGE_USAGE_SAMPLED_BIT;
            }
            if (isDepthStencil)
            {
                vkUsage |= VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT;
            }
            if ((vdUsage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget)
            {
                vkUsage |= VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
            }
            if ((vdUsage & TextureUsage.Storage) == TextureUsage.Storage)
            {
                vkUsage |= VK_IMAGE_USAGE_STORAGE_BIT;
            }

            return vkUsage;
        }

        internal static VkImageType VdToVkTextureType(TextureType type)
        {
            return type switch
            {
                TextureType.Texture1D => VK_IMAGE_TYPE_1D,
                TextureType.Texture2D => VK_IMAGE_TYPE_2D,
                TextureType.Texture3D => VK_IMAGE_TYPE_3D,
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
                    return dynamicBinding ? VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC : VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
                case ResourceKind.StructuredBufferReadWrite:
                case ResourceKind.StructuredBufferReadOnly:
                    return dynamicBinding ? VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC : VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
                case ResourceKind.TextureReadOnly:
                    return VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
                case ResourceKind.TextureReadWrite:
                    return VK_DESCRIPTOR_TYPE_STORAGE_IMAGE;
                case ResourceKind.Sampler:
                    return VK_DESCRIPTOR_TYPE_SAMPLER;
                default:
                    throw Illegal.Value<ResourceKind>();
            }
        }

        internal static VkSampleCountFlags VdToVkSampleCount(TextureSampleCount sampleCount)
        {
            return sampleCount switch
            {
                TextureSampleCount.Count1 => VK_SAMPLE_COUNT_1_BIT,
                TextureSampleCount.Count2 => VK_SAMPLE_COUNT_2_BIT,
                TextureSampleCount.Count4 => VK_SAMPLE_COUNT_4_BIT,
                TextureSampleCount.Count8 => VK_SAMPLE_COUNT_8_BIT,
                TextureSampleCount.Count16 => VK_SAMPLE_COUNT_16_BIT,
                TextureSampleCount.Count32 => VK_SAMPLE_COUNT_32_BIT,
                _ => throw Illegal.Value<TextureSampleCount>(),
            };
        }

        internal static VkStencilOp VdToVkStencilOp(StencilOperation op)
        {
            return op switch
            {
                StencilOperation.Keep => VK_STENCIL_OP_KEEP,
                StencilOperation.Zero => VK_STENCIL_OP_ZERO,
                StencilOperation.Replace => VK_STENCIL_OP_REPLACE,
                StencilOperation.IncrementAndClamp => VK_STENCIL_OP_INCREMENT_AND_CLAMP,
                StencilOperation.DecrementAndClamp => VK_STENCIL_OP_DECREMENT_AND_CLAMP,
                StencilOperation.Invert => VK_STENCIL_OP_INVERT,
                StencilOperation.IncrementAndWrap => VK_STENCIL_OP_INCREMENT_AND_WRAP,
                StencilOperation.DecrementAndWrap => VK_STENCIL_OP_DECREMENT_AND_WRAP,
                _ => throw Illegal.Value<StencilOperation>(),
            };
        }

        internal static VkPolygonMode VdToVkPolygonMode(PolygonFillMode fillMode)
        {
            return fillMode switch
            {
                PolygonFillMode.Solid => VK_POLYGON_MODE_FILL,
                PolygonFillMode.Wireframe => VK_POLYGON_MODE_LINE,
                _ => throw Illegal.Value<PolygonFillMode>(),
            };
        }

        internal static VkCullModeFlags VdToVkCullMode(FaceCullMode cullMode)
        {
            return cullMode switch
            {
                FaceCullMode.Back => VK_CULL_MODE_BACK_BIT,
                FaceCullMode.Front => VK_CULL_MODE_FRONT_BIT,
                FaceCullMode.None => VK_CULL_MODE_NONE,
                _ => throw Illegal.Value<FaceCullMode>(),
            };
        }

        internal static VkBlendOp VdToVkBlendOp(BlendFunction func)
        {
            return func switch
            {
                BlendFunction.Add => VK_BLEND_OP_ADD,
                BlendFunction.Subtract => VK_BLEND_OP_SUBTRACT,
                BlendFunction.ReverseSubtract => VK_BLEND_OP_REVERSE_SUBTRACT,
                BlendFunction.Minimum => VK_BLEND_OP_MIN,
                BlendFunction.Maximum => VK_BLEND_OP_MAX,
                _ => throw Illegal.Value<BlendFunction>(),
            };
        }

        internal static VkColorComponentFlags VdToVkColorWriteMask(ColorWriteMask mask)
        {
            VkColorComponentFlags flags = default;

            if ((mask & ColorWriteMask.Red) == ColorWriteMask.Red)
                flags |= VkColorComponentFlags.VK_COLOR_COMPONENT_R_BIT;
            if ((mask & ColorWriteMask.Green) == ColorWriteMask.Green)
                flags |= VkColorComponentFlags.VK_COLOR_COMPONENT_G_BIT;
            if ((mask & ColorWriteMask.Blue) == ColorWriteMask.Blue)
                flags |= VkColorComponentFlags.VK_COLOR_COMPONENT_B_BIT;
            if ((mask & ColorWriteMask.Alpha) == ColorWriteMask.Alpha)
                flags |= VkColorComponentFlags.VK_COLOR_COMPONENT_A_BIT;

            return flags;
        }

        internal static VkPrimitiveTopology VdToVkPrimitiveTopology(PrimitiveTopology topology)
        {
            return topology switch
            {
                PrimitiveTopology.TriangleList => VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST,
                PrimitiveTopology.TriangleStrip => VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP,
                PrimitiveTopology.LineList => VK_PRIMITIVE_TOPOLOGY_LINE_LIST,
                PrimitiveTopology.LineStrip => VK_PRIMITIVE_TOPOLOGY_LINE_STRIP,
                PrimitiveTopology.PointList => VK_PRIMITIVE_TOPOLOGY_POINT_LIST,
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
                BlendFactor.Zero => VK_BLEND_FACTOR_ZERO,
                BlendFactor.One => VK_BLEND_FACTOR_ONE,
                BlendFactor.SourceAlpha => VK_BLEND_FACTOR_SRC_ALPHA,
                BlendFactor.InverseSourceAlpha => VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA,
                BlendFactor.DestinationAlpha => VK_BLEND_FACTOR_DST_ALPHA,
                BlendFactor.InverseDestinationAlpha => VK_BLEND_FACTOR_ONE_MINUS_DST_ALPHA,
                BlendFactor.SourceColor => VK_BLEND_FACTOR_SRC_COLOR,
                BlendFactor.InverseSourceColor => VK_BLEND_FACTOR_ONE_MINUS_SRC_COLOR,
                BlendFactor.DestinationColor => VK_BLEND_FACTOR_DST_COLOR,
                BlendFactor.InverseDestinationColor => VK_BLEND_FACTOR_ONE_MINUS_DST_COLOR,
                BlendFactor.BlendFactor => VK_BLEND_FACTOR_CONSTANT_COLOR,
                BlendFactor.InverseBlendFactor => VK_BLEND_FACTOR_ONE_MINUS_CONSTANT_COLOR,
                _ => throw Illegal.Value<BlendFactor>(),
            };
        }

        internal static VkFormat VdToVkVertexElementFormat(VertexElementFormat format)
        {
            return format switch
            {
                VertexElementFormat.Float1 => VK_FORMAT_R32_SFLOAT,
                VertexElementFormat.Float2 => VK_FORMAT_R32G32_SFLOAT,
                VertexElementFormat.Float3 => VK_FORMAT_R32G32B32_SFLOAT,
                VertexElementFormat.Float4 => VK_FORMAT_R32G32B32A32_SFLOAT,
                VertexElementFormat.Byte2_Norm => VK_FORMAT_R8G8_UNORM,
                VertexElementFormat.Byte2 => VK_FORMAT_R8G8_UINT,
                VertexElementFormat.Byte4_Norm => VK_FORMAT_R8G8B8A8_UNORM,
                VertexElementFormat.Byte4 => VK_FORMAT_R8G8B8A8_UINT,
                VertexElementFormat.SByte2_Norm => VK_FORMAT_R8G8_SNORM,
                VertexElementFormat.SByte2 => VK_FORMAT_R8G8_SINT,
                VertexElementFormat.SByte4_Norm => VK_FORMAT_R8G8B8A8_SNORM,
                VertexElementFormat.SByte4 => VK_FORMAT_R8G8B8A8_SINT,
                VertexElementFormat.UShort2_Norm => VK_FORMAT_R16G16_UNORM,
                VertexElementFormat.UShort2 => VK_FORMAT_R16G16_UINT,
                VertexElementFormat.UShort4_Norm => VK_FORMAT_R16G16B16A16_UNORM,
                VertexElementFormat.UShort4 => VK_FORMAT_R16G16B16A16_UINT,
                VertexElementFormat.Short2_Norm => VK_FORMAT_R16G16_SNORM,
                VertexElementFormat.Short2 => VK_FORMAT_R16G16_SINT,
                VertexElementFormat.Short4_Norm => VK_FORMAT_R16G16B16A16_SNORM,
                VertexElementFormat.Short4 => VK_FORMAT_R16G16B16A16_SINT,
                VertexElementFormat.UInt1 => VK_FORMAT_R32_UINT,
                VertexElementFormat.UInt2 => VK_FORMAT_R32G32_UINT,
                VertexElementFormat.UInt3 => VK_FORMAT_R32G32B32_UINT,
                VertexElementFormat.UInt4 => VK_FORMAT_R32G32B32A32_UINT,
                VertexElementFormat.Int1 => VK_FORMAT_R32_SINT,
                VertexElementFormat.Int2 => VK_FORMAT_R32G32_SINT,
                VertexElementFormat.Int3 => VK_FORMAT_R32G32B32_SINT,
                VertexElementFormat.Int4 => VK_FORMAT_R32G32B32A32_SINT,
                VertexElementFormat.Half1 => VK_FORMAT_R16_SFLOAT,
                VertexElementFormat.Half2 => VK_FORMAT_R16G16_SFLOAT,
                VertexElementFormat.Half4 => VK_FORMAT_R16G16B16A16_SFLOAT,
                _ => throw Illegal.Value<VertexElementFormat>(),
            };
        }

        internal static VkShaderStageFlags VdToVkShaderStages(ShaderStages stage)
        {
            VkShaderStageFlags ret = 0;

            if ((stage & ShaderStages.Vertex) == ShaderStages.Vertex)
                ret |= VK_SHADER_STAGE_VERTEX_BIT;

            if ((stage & ShaderStages.Geometry) == ShaderStages.Geometry)
                ret |= VK_SHADER_STAGE_GEOMETRY_BIT;

            if ((stage & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
                ret |= VK_SHADER_STAGE_TESSELLATION_CONTROL_BIT;

            if ((stage & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
                ret |= VK_SHADER_STAGE_TESSELLATION_EVALUATION_BIT;

            if ((stage & ShaderStages.Fragment) == ShaderStages.Fragment)
                ret |= VK_SHADER_STAGE_FRAGMENT_BIT;

            if ((stage & ShaderStages.Compute) == ShaderStages.Compute)
                ret |= VK_SHADER_STAGE_COMPUTE_BIT;

            return ret;
        }

        internal static VkBorderColor VdToVkSamplerBorderColor(SamplerBorderColor borderColor)
        {
            return borderColor switch
            {
                SamplerBorderColor.TransparentBlack => VK_BORDER_COLOR_FLOAT_TRANSPARENT_BLACK,
                SamplerBorderColor.OpaqueBlack => VK_BORDER_COLOR_FLOAT_OPAQUE_BLACK,
                SamplerBorderColor.OpaqueWhite => VK_BORDER_COLOR_FLOAT_OPAQUE_WHITE,
                _ => throw Illegal.Value<SamplerBorderColor>(),
            };
        }

        internal static VkIndexType VdToVkIndexFormat(IndexFormat format)
        {
            return format switch
            {
                IndexFormat.UInt16 => VK_INDEX_TYPE_UINT16,
                IndexFormat.UInt32 => VK_INDEX_TYPE_UINT32,
                _ => throw Illegal.Value<IndexFormat>(),
            };
        }

        internal static VkCompareOp VdToVkCompareOp(ComparisonKind comparisonKind)
        {
            return comparisonKind switch
            {
                ComparisonKind.Never => VK_COMPARE_OP_NEVER,
                ComparisonKind.Less => VK_COMPARE_OP_LESS,
                ComparisonKind.Equal => VK_COMPARE_OP_EQUAL,
                ComparisonKind.LessEqual => VK_COMPARE_OP_LESS_OR_EQUAL,
                ComparisonKind.Greater => VK_COMPARE_OP_GREATER,
                ComparisonKind.NotEqual => VK_COMPARE_OP_NOT_EQUAL,
                ComparisonKind.GreaterEqual => VK_COMPARE_OP_GREATER_OR_EQUAL,
                ComparisonKind.Always => VK_COMPARE_OP_ALWAYS,
                _ => throw Illegal.Value<ComparisonKind>(),
            };
        }

        internal static PixelFormat VkToVdPixelFormat(VkFormat vkFormat)
        {
            return vkFormat switch
            {
                VK_FORMAT_R8_UNORM => PixelFormat.R8_UNorm,
                VK_FORMAT_R8_SNORM => PixelFormat.R8_SNorm,
                VK_FORMAT_R8_UINT => PixelFormat.R8_UInt,
                VK_FORMAT_R8_SINT => PixelFormat.R8_SInt,
                VK_FORMAT_R16_UNORM => PixelFormat.R16_UNorm,
                VK_FORMAT_R16_SNORM => PixelFormat.R16_SNorm,
                VK_FORMAT_R16_UINT => PixelFormat.R16_UInt,
                VK_FORMAT_R16_SINT => PixelFormat.R16_SInt,
                VK_FORMAT_R16_SFLOAT => PixelFormat.R16_Float,
                VK_FORMAT_R32_UINT => PixelFormat.R32_UInt,
                VK_FORMAT_R32_SINT => PixelFormat.R32_SInt,
                VK_FORMAT_R32_SFLOAT or VK_FORMAT_D32_SFLOAT => PixelFormat.R32_Float,
                VK_FORMAT_R8G8_UNORM => PixelFormat.R8_G8_UNorm,
                VK_FORMAT_R8G8_SNORM => PixelFormat.R8_G8_SNorm,
                VK_FORMAT_R8G8_UINT => PixelFormat.R8_G8_UInt,
                VK_FORMAT_R8G8_SINT => PixelFormat.R8_G8_SInt,
                VK_FORMAT_R16G16_UNORM => PixelFormat.R16_G16_UNorm,
                VK_FORMAT_R16G16_SNORM => PixelFormat.R16_G16_SNorm,
                VK_FORMAT_R16G16_UINT => PixelFormat.R16_G16_UInt,
                VK_FORMAT_R16G16_SINT => PixelFormat.R16_G16_SInt,
                VK_FORMAT_R16G16_SFLOAT => PixelFormat.R16_G16_Float,
                VK_FORMAT_R32G32_UINT => PixelFormat.R32_G32_UInt,
                VK_FORMAT_R32G32_SINT => PixelFormat.R32_G32_SInt,
                VK_FORMAT_R32G32_SFLOAT => PixelFormat.R32_G32_Float,
                VK_FORMAT_R8G8B8A8_UNORM => PixelFormat.R8_G8_B8_A8_UNorm,
                VK_FORMAT_R8G8B8A8_SRGB => PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                VK_FORMAT_B8G8R8A8_UNORM => PixelFormat.B8_G8_R8_A8_UNorm,
                VK_FORMAT_B8G8R8A8_SRGB => PixelFormat.B8_G8_R8_A8_UNorm_SRgb,
                VK_FORMAT_R8G8B8A8_SNORM => PixelFormat.R8_G8_B8_A8_SNorm,
                VK_FORMAT_R8G8B8A8_UINT => PixelFormat.R8_G8_B8_A8_UInt,
                VK_FORMAT_R8G8B8A8_SINT => PixelFormat.R8_G8_B8_A8_SInt,
                VK_FORMAT_R16G16B16A16_UNORM => PixelFormat.R16_G16_B16_A16_UNorm,
                VK_FORMAT_R16G16B16A16_SNORM => PixelFormat.R16_G16_B16_A16_SNorm,
                VK_FORMAT_R16G16B16A16_UINT => PixelFormat.R16_G16_B16_A16_UInt,
                VK_FORMAT_R16G16B16A16_SINT => PixelFormat.R16_G16_B16_A16_SInt,
                VK_FORMAT_R16G16B16A16_SFLOAT => PixelFormat.R16_G16_B16_A16_Float,
                VK_FORMAT_R32G32B32A32_UINT => PixelFormat.R32_G32_B32_A32_UInt,
                VK_FORMAT_R32G32B32A32_SINT => PixelFormat.R32_G32_B32_A32_SInt,
                VK_FORMAT_R32G32B32A32_SFLOAT => PixelFormat.R32_G32_B32_A32_Float,
                VK_FORMAT_BC1_RGB_UNORM_BLOCK => PixelFormat.BC1_Rgb_UNorm,
                VK_FORMAT_BC1_RGB_SRGB_BLOCK => PixelFormat.BC1_Rgb_UNorm_SRgb,
                VK_FORMAT_BC1_RGBA_UNORM_BLOCK => PixelFormat.BC1_Rgba_UNorm,
                VK_FORMAT_BC1_RGBA_SRGB_BLOCK => PixelFormat.BC1_Rgba_UNorm_SRgb,
                VK_FORMAT_BC2_UNORM_BLOCK => PixelFormat.BC2_UNorm,
                VK_FORMAT_BC2_SRGB_BLOCK => PixelFormat.BC2_UNorm_SRgb,
                VK_FORMAT_BC3_UNORM_BLOCK => PixelFormat.BC3_UNorm,
                VK_FORMAT_BC3_SRGB_BLOCK => PixelFormat.BC3_UNorm_SRgb,
                VK_FORMAT_BC4_UNORM_BLOCK => PixelFormat.BC4_UNorm,
                VK_FORMAT_BC4_SNORM_BLOCK => PixelFormat.BC4_SNorm,
                VK_FORMAT_BC5_UNORM_BLOCK => PixelFormat.BC5_UNorm,
                VK_FORMAT_BC5_SNORM_BLOCK => PixelFormat.BC5_SNorm,
                VK_FORMAT_BC7_UNORM_BLOCK => PixelFormat.BC7_UNorm,
                VK_FORMAT_BC7_SRGB_BLOCK => PixelFormat.BC7_UNorm_SRgb,
                VK_FORMAT_A2B10G10R10_UNORM_PACK32 => PixelFormat.R10_G10_B10_A2_UNorm,
                VK_FORMAT_A2B10G10R10_UINT_PACK32 => PixelFormat.R10_G10_B10_A2_UInt,
                VK_FORMAT_B10G11R11_UFLOAT_PACK32 => PixelFormat.R11_G11_B10_Float,
                _ => throw Illegal.Value<VkFormat>(),
            };
        }
    }
}
