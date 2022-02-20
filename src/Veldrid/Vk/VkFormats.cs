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
            switch (mode)
            {
                case SamplerAddressMode.Wrap:
                    return VK_SAMPLER_ADDRESS_MODE_REPEAT;
                case SamplerAddressMode.Mirror:
                    return VK_SAMPLER_ADDRESS_MODE_MIRRORED_REPEAT;
                case SamplerAddressMode.Clamp:
                    return VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_EDGE;
                case SamplerAddressMode.Border:
                    return VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_BORDER;
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
            switch (type)
            {
                case TextureType.Texture1D:
                    return VK_IMAGE_TYPE_1D;
                case TextureType.Texture2D:
                    return VK_IMAGE_TYPE_2D;
                case TextureType.Texture3D:
                    return VK_IMAGE_TYPE_3D;
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
            switch (sampleCount)
            {
                case TextureSampleCount.Count1:
                    return VK_SAMPLE_COUNT_1_BIT;
                case TextureSampleCount.Count2:
                    return VK_SAMPLE_COUNT_2_BIT;
                case TextureSampleCount.Count4:
                    return VK_SAMPLE_COUNT_4_BIT;
                case TextureSampleCount.Count8:
                    return VK_SAMPLE_COUNT_8_BIT;
                case TextureSampleCount.Count16:
                    return VK_SAMPLE_COUNT_16_BIT;
                case TextureSampleCount.Count32:
                    return VK_SAMPLE_COUNT_32_BIT;
                default:
                    throw Illegal.Value<TextureSampleCount>();
            }
        }

        internal static VkStencilOp VdToVkStencilOp(StencilOperation op)
        {
            switch (op)
            {
                case StencilOperation.Keep:
                    return VK_STENCIL_OP_KEEP;
                case StencilOperation.Zero:
                    return VK_STENCIL_OP_ZERO;
                case StencilOperation.Replace:
                    return VK_STENCIL_OP_REPLACE;
                case StencilOperation.IncrementAndClamp:
                    return VK_STENCIL_OP_INCREMENT_AND_CLAMP;
                case StencilOperation.DecrementAndClamp:
                    return VK_STENCIL_OP_DECREMENT_AND_CLAMP;
                case StencilOperation.Invert:
                    return VK_STENCIL_OP_INVERT;
                case StencilOperation.IncrementAndWrap:
                    return VK_STENCIL_OP_INCREMENT_AND_WRAP;
                case StencilOperation.DecrementAndWrap:
                    return VK_STENCIL_OP_DECREMENT_AND_WRAP;
                default:
                    throw Illegal.Value<StencilOperation>();
            }
        }

        internal static VkPolygonMode VdToVkPolygonMode(PolygonFillMode fillMode)
        {
            switch (fillMode)
            {
                case PolygonFillMode.Solid:
                    return VK_POLYGON_MODE_FILL;
                case PolygonFillMode.Wireframe:
                    return VK_POLYGON_MODE_LINE;
                default:
                    throw Illegal.Value<PolygonFillMode>();
            }
        }

        internal static VkCullModeFlags VdToVkCullMode(FaceCullMode cullMode)
        {
            switch (cullMode)
            {
                case FaceCullMode.Back:
                    return VK_CULL_MODE_BACK_BIT;
                case FaceCullMode.Front:
                    return VK_CULL_MODE_FRONT_BIT;
                case FaceCullMode.None:
                    return VK_CULL_MODE_NONE;
                default:
                    throw Illegal.Value<FaceCullMode>();
            }
        }

        internal static VkBlendOp VdToVkBlendOp(BlendFunction func)
        {
            switch (func)
            {
                case BlendFunction.Add:
                    return VK_BLEND_OP_ADD;
                case BlendFunction.Subtract:
                    return VK_BLEND_OP_SUBTRACT;
                case BlendFunction.ReverseSubtract:
                    return VK_BLEND_OP_REVERSE_SUBTRACT;
                case BlendFunction.Minimum:
                    return VK_BLEND_OP_MIN;
                case BlendFunction.Maximum:
                    return VK_BLEND_OP_MAX;
                default:
                    throw Illegal.Value<BlendFunction>();
            }
        }

        internal static VkPrimitiveTopology VdToVkPrimitiveTopology(PrimitiveTopology topology)
        {
            switch (topology)
            {
                case PrimitiveTopology.TriangleList:
                    return VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
                case PrimitiveTopology.TriangleStrip:
                    return VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP;
                case PrimitiveTopology.LineList:
                    return VK_PRIMITIVE_TOPOLOGY_LINE_LIST;
                case PrimitiveTopology.LineStrip:
                    return VK_PRIMITIVE_TOPOLOGY_LINE_STRIP;
                case PrimitiveTopology.PointList:
                    return VK_PRIMITIVE_TOPOLOGY_POINT_LIST;
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
                    return VK_BLEND_FACTOR_ZERO;
                case BlendFactor.One:
                    return VK_BLEND_FACTOR_ONE;
                case BlendFactor.SourceAlpha:
                    return VK_BLEND_FACTOR_SRC_ALPHA;
                case BlendFactor.InverseSourceAlpha:
                    return VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA;
                case BlendFactor.DestinationAlpha:
                    return VK_BLEND_FACTOR_DST_ALPHA;
                case BlendFactor.InverseDestinationAlpha:
                    return VK_BLEND_FACTOR_ONE_MINUS_DST_ALPHA;
                case BlendFactor.SourceColor:
                    return VK_BLEND_FACTOR_SRC_COLOR;
                case BlendFactor.InverseSourceColor:
                    return VK_BLEND_FACTOR_ONE_MINUS_SRC_COLOR;
                case BlendFactor.DestinationColor:
                    return VK_BLEND_FACTOR_DST_COLOR;
                case BlendFactor.InverseDestinationColor:
                    return VK_BLEND_FACTOR_ONE_MINUS_DST_COLOR;
                case BlendFactor.BlendFactor:
                    return VK_BLEND_FACTOR_CONSTANT_COLOR;
                case BlendFactor.InverseBlendFactor:
                    return VK_BLEND_FACTOR_ONE_MINUS_CONSTANT_COLOR;
                default:
                    throw Illegal.Value<BlendFactor>();
            }
        }

        internal static VkFormat VdToVkVertexElementFormat(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                    return VK_FORMAT_R32_SFLOAT;
                case VertexElementFormat.Float2:
                    return VK_FORMAT_R32G32_SFLOAT;
                case VertexElementFormat.Float3:
                    return VK_FORMAT_R32G32B32_SFLOAT;
                case VertexElementFormat.Float4:
                    return VK_FORMAT_R32G32B32A32_SFLOAT;
                case VertexElementFormat.Byte2_Norm:
                    return VK_FORMAT_R8G8_UNORM;
                case VertexElementFormat.Byte2:
                    return VK_FORMAT_R8G8_UINT;
                case VertexElementFormat.Byte4_Norm:
                    return VK_FORMAT_R8G8B8A8_UNORM;
                case VertexElementFormat.Byte4:
                    return VK_FORMAT_R8G8B8A8_UINT;
                case VertexElementFormat.SByte2_Norm:
                    return VK_FORMAT_R8G8_SNORM;
                case VertexElementFormat.SByte2:
                    return VK_FORMAT_R8G8_SINT;
                case VertexElementFormat.SByte4_Norm:
                    return VK_FORMAT_R8G8B8A8_SNORM;
                case VertexElementFormat.SByte4:
                    return VK_FORMAT_R8G8B8A8_SINT;
                case VertexElementFormat.UShort2_Norm:
                    return VK_FORMAT_R16G16_UNORM;
                case VertexElementFormat.UShort2:
                    return VK_FORMAT_R16G16_UINT;
                case VertexElementFormat.UShort4_Norm:
                    return VK_FORMAT_R16G16B16A16_UNORM;
                case VertexElementFormat.UShort4:
                    return VK_FORMAT_R16G16B16A16_UINT;
                case VertexElementFormat.Short2_Norm:
                    return VK_FORMAT_R16G16_SNORM;
                case VertexElementFormat.Short2:
                    return VK_FORMAT_R16G16_SINT;
                case VertexElementFormat.Short4_Norm:
                    return VK_FORMAT_R16G16B16A16_SNORM;
                case VertexElementFormat.Short4:
                    return VK_FORMAT_R16G16B16A16_SINT;
                case VertexElementFormat.UInt1:
                    return VK_FORMAT_R32_UINT;
                case VertexElementFormat.UInt2:
                    return VK_FORMAT_R32G32_UINT;
                case VertexElementFormat.UInt3:
                    return VK_FORMAT_R32G32B32_UINT;
                case VertexElementFormat.UInt4:
                    return VK_FORMAT_R32G32B32A32_UINT;
                case VertexElementFormat.Int1:
                    return VK_FORMAT_R32_SINT;
                case VertexElementFormat.Int2:
                    return VK_FORMAT_R32G32_SINT;
                case VertexElementFormat.Int3:
                    return VK_FORMAT_R32G32B32_SINT;
                case VertexElementFormat.Int4:
                    return VK_FORMAT_R32G32B32A32_SINT;
                case VertexElementFormat.Half1:
                    return VK_FORMAT_R16_SFLOAT;
                case VertexElementFormat.Half2:
                    return VK_FORMAT_R16G16_SFLOAT;
                case VertexElementFormat.Half4:
                    return VK_FORMAT_R16G16B16A16_SFLOAT;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
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
            switch (borderColor)
            {
                case SamplerBorderColor.TransparentBlack:
                    return VK_BORDER_COLOR_FLOAT_TRANSPARENT_BLACK;
                case SamplerBorderColor.OpaqueBlack:
                    return VK_BORDER_COLOR_FLOAT_OPAQUE_BLACK;
                case SamplerBorderColor.OpaqueWhite:
                    return VK_BORDER_COLOR_FLOAT_OPAQUE_WHITE;
                default:
                    throw Illegal.Value<SamplerBorderColor>();
            }
        }

        internal static VkIndexType VdToVkIndexFormat(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt16:
                    return VK_INDEX_TYPE_UINT16;
                case IndexFormat.UInt32:
                    return VK_INDEX_TYPE_UINT32;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        internal static VkCompareOp VdToVkCompareOp(ComparisonKind comparisonKind)
        {
            switch (comparisonKind)
            {
                case ComparisonKind.Never:
                    return VK_COMPARE_OP_NEVER;
                case ComparisonKind.Less:
                    return VK_COMPARE_OP_LESS;
                case ComparisonKind.Equal:
                    return VK_COMPARE_OP_EQUAL;
                case ComparisonKind.LessEqual:
                    return VK_COMPARE_OP_LESS_OR_EQUAL;
                case ComparisonKind.Greater:
                    return VK_COMPARE_OP_GREATER;
                case ComparisonKind.NotEqual:
                    return VK_COMPARE_OP_NOT_EQUAL;
                case ComparisonKind.GreaterEqual:
                    return VK_COMPARE_OP_GREATER_OR_EQUAL;
                case ComparisonKind.Always:
                    return VK_COMPARE_OP_ALWAYS;
                default:
                    throw Illegal.Value<ComparisonKind>();
            }
        }

        internal static PixelFormat VkToVdPixelFormat(VkFormat vkFormat)
        {
            switch (vkFormat)
            {
                case VK_FORMAT_R8_UNORM:
                    return PixelFormat.R8_UNorm;
                case VK_FORMAT_R8_SNORM:
                    return PixelFormat.R8_SNorm;
                case VK_FORMAT_R8_UINT:
                    return PixelFormat.R8_UInt;
                case VK_FORMAT_R8_SINT:
                    return PixelFormat.R8_SInt;

                case VK_FORMAT_R16_UNORM:
                    return PixelFormat.R16_UNorm;
                case VK_FORMAT_R16_SNORM:
                    return PixelFormat.R16_SNorm;
                case VK_FORMAT_R16_UINT:
                    return PixelFormat.R16_UInt;
                case VK_FORMAT_R16_SINT:
                    return PixelFormat.R16_SInt;
                case VK_FORMAT_R16_SFLOAT:
                    return PixelFormat.R16_Float;

                case VK_FORMAT_R32_UINT:
                    return PixelFormat.R32_UInt;
                case VK_FORMAT_R32_SINT:
                    return PixelFormat.R32_SInt;
                case VK_FORMAT_R32_SFLOAT:
                case VK_FORMAT_D32_SFLOAT:
                    return PixelFormat.R32_Float;

                case VK_FORMAT_R8G8_UNORM:
                    return PixelFormat.R8_G8_UNorm;
                case VK_FORMAT_R8G8_SNORM:
                    return PixelFormat.R8_G8_SNorm;
                case VK_FORMAT_R8G8_UINT:
                    return PixelFormat.R8_G8_UInt;
                case VK_FORMAT_R8G8_SINT:
                    return PixelFormat.R8_G8_SInt;

                case VK_FORMAT_R16G16_UNORM:
                    return PixelFormat.R16_G16_UNorm;
                case VK_FORMAT_R16G16_SNORM:
                    return PixelFormat.R16_G16_SNorm;
                case VK_FORMAT_R16G16_UINT:
                    return PixelFormat.R16_G16_UInt;
                case VK_FORMAT_R16G16_SINT:
                    return PixelFormat.R16_G16_SInt;
                case VK_FORMAT_R16G16_SFLOAT:
                    return PixelFormat.R16_G16_Float;

                case VK_FORMAT_R32G32_UINT:
                    return PixelFormat.R32_G32_UInt;
                case VK_FORMAT_R32G32_SINT:
                    return PixelFormat.R32_G32_SInt;
                case VK_FORMAT_R32G32_SFLOAT:
                    return PixelFormat.R32_G32_Float;

                case VK_FORMAT_R8G8B8A8_UNORM:
                    return PixelFormat.R8_G8_B8_A8_UNorm;
                case VK_FORMAT_R8G8B8A8_SRGB:
                    return PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
                case VK_FORMAT_B8G8R8A8_UNORM:
                    return PixelFormat.B8_G8_R8_A8_UNorm;
                case VK_FORMAT_B8G8R8A8_SRGB:
                    return PixelFormat.B8_G8_R8_A8_UNorm_SRgb;
                case VK_FORMAT_R8G8B8A8_SNORM:
                    return PixelFormat.R8_G8_B8_A8_SNorm;
                case VK_FORMAT_R8G8B8A8_UINT:
                    return PixelFormat.R8_G8_B8_A8_UInt;
                case VK_FORMAT_R8G8B8A8_SINT:
                    return PixelFormat.R8_G8_B8_A8_SInt;

                case VK_FORMAT_R16G16B16A16_UNORM:
                    return PixelFormat.R16_G16_B16_A16_UNorm;
                case VK_FORMAT_R16G16B16A16_SNORM:
                    return PixelFormat.R16_G16_B16_A16_SNorm;
                case VK_FORMAT_R16G16B16A16_UINT:
                    return PixelFormat.R16_G16_B16_A16_UInt;
                case VK_FORMAT_R16G16B16A16_SINT:
                    return PixelFormat.R16_G16_B16_A16_SInt;
                case VK_FORMAT_R16G16B16A16_SFLOAT:
                    return PixelFormat.R16_G16_B16_A16_Float;

                case VK_FORMAT_R32G32B32A32_UINT:
                    return PixelFormat.R32_G32_B32_A32_UInt;
                case VK_FORMAT_R32G32B32A32_SINT:
                    return PixelFormat.R32_G32_B32_A32_SInt;
                case VK_FORMAT_R32G32B32A32_SFLOAT:
                    return PixelFormat.R32_G32_B32_A32_Float;

                case VK_FORMAT_BC1_RGB_UNORM_BLOCK:
                    return PixelFormat.BC1_Rgb_UNorm;
                case VK_FORMAT_BC1_RGB_SRGB_BLOCK:
                    return PixelFormat.BC1_Rgb_UNorm_SRgb;
                case VK_FORMAT_BC1_RGBA_UNORM_BLOCK:
                    return PixelFormat.BC1_Rgba_UNorm;
                case VK_FORMAT_BC1_RGBA_SRGB_BLOCK:
                    return PixelFormat.BC1_Rgba_UNorm_SRgb;
                case VK_FORMAT_BC2_UNORM_BLOCK:
                    return PixelFormat.BC2_UNorm;
                case VK_FORMAT_BC2_SRGB_BLOCK:
                    return PixelFormat.BC2_UNorm_SRgb;
                case VK_FORMAT_BC3_UNORM_BLOCK:
                    return PixelFormat.BC3_UNorm;
                case VK_FORMAT_BC3_SRGB_BLOCK:
                    return PixelFormat.BC3_UNorm_SRgb;
                case VK_FORMAT_BC4_UNORM_BLOCK:
                    return PixelFormat.BC4_UNorm;
                case VK_FORMAT_BC4_SNORM_BLOCK:
                    return PixelFormat.BC4_SNorm;
                case VK_FORMAT_BC5_UNORM_BLOCK:
                    return PixelFormat.BC5_UNorm;
                case VK_FORMAT_BC5_SNORM_BLOCK:
                    return PixelFormat.BC5_SNorm;
                case VK_FORMAT_BC7_UNORM_BLOCK:
                    return PixelFormat.BC7_UNorm;
                case VK_FORMAT_BC7_SRGB_BLOCK:
                    return PixelFormat.BC7_UNorm_SRgb;

                case VK_FORMAT_A2B10G10R10_UNORM_PACK32:
                    return PixelFormat.R10_G10_B10_A2_UNorm;
                case VK_FORMAT_A2B10G10R10_UINT_PACK32:
                    return PixelFormat.R10_G10_B10_A2_UInt;
                case VK_FORMAT_B10G11R11_UFLOAT_PACK32:
                    return PixelFormat.R11_G11_B10_Float;

                default:
                    throw Illegal.Value<VkFormat>();
            }
        }
    }
}
