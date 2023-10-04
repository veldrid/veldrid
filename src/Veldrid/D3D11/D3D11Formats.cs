using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Veldrid.D3D11
{
    internal static class D3D11Formats
    {
        internal static Format ToDxgiFormat(PixelFormat format, bool depthFormat)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                    return Format.R8_UNorm;
                case PixelFormat.R8_SNorm:
                    return Format.R8_SNorm;
                case PixelFormat.R8_UInt:
                    return Format.R8_UInt;
                case PixelFormat.R8_SInt:
                    return Format.R8_SInt;

                case PixelFormat.R16_UNorm:
                    return depthFormat ? Format.R16_Typeless : Format.R16_UNorm;
                case PixelFormat.R16_SNorm:
                    return Format.R16_SNorm;
                case PixelFormat.R16_UInt:
                    return Format.R16_UInt;
                case PixelFormat.R16_SInt:
                    return Format.R16_SInt;
                case PixelFormat.R16_Float:
                    return Format.R16_Float;

                case PixelFormat.R32_UInt:
                    return Format.R32_UInt;
                case PixelFormat.R32_SInt:
                    return Format.R32_SInt;
                case PixelFormat.R32_Float:
                    return depthFormat ? Format.R32_Typeless : Format.R32_Float;

                case PixelFormat.R8_G8_UNorm:
                    return Format.R8G8_UNorm;
                case PixelFormat.R8_G8_SNorm:
                    return Format.R8G8_SNorm;
                case PixelFormat.R8_G8_UInt:
                    return Format.R8G8_UInt;
                case PixelFormat.R8_G8_SInt:
                    return Format.R8G8_SInt;

                case PixelFormat.R16_G16_UNorm:
                    return Format.R16G16_UNorm;
                case PixelFormat.R16_G16_SNorm:
                    return Format.R16G16_SNorm;
                case PixelFormat.R16_G16_UInt:
                    return Format.R16G16_UInt;
                case PixelFormat.R16_G16_SInt:
                    return Format.R16G16_SInt;
                case PixelFormat.R16_G16_Float:
                    return Format.R16G16_Float;

                case PixelFormat.R32_G32_UInt:
                    return Format.R32G32_UInt;
                case PixelFormat.R32_G32_SInt:
                    return Format.R32G32_SInt;
                case PixelFormat.R32_G32_Float:
                    return Format.R32G32_Float;

                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return Format.R8G8B8A8_UNorm;
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                    return Format.R8G8B8A8_UNorm_SRgb;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return Format.B8G8R8A8_UNorm;
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return Format.B8G8R8A8_UNorm_SRgb;
                case PixelFormat.R8_G8_B8_A8_SNorm:
                    return Format.R8G8B8A8_SNorm;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return Format.R8G8B8A8_UInt;
                case PixelFormat.R8_G8_B8_A8_SInt:
                    return Format.R8G8B8A8_SInt;

                case PixelFormat.R16_G16_B16_A16_UNorm:
                    return Format.R16G16B16A16_UNorm;
                case PixelFormat.R16_G16_B16_A16_SNorm:
                    return Format.R16G16B16A16_SNorm;
                case PixelFormat.R16_G16_B16_A16_UInt:
                    return Format.R16G16B16A16_UInt;
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return Format.R16G16B16A16_SInt;
                case PixelFormat.R16_G16_B16_A16_Float:
                    return Format.R16G16B16A16_Float;

                case PixelFormat.R32_G32_B32_A32_UInt:
                    return Format.R32G32B32A32_UInt;
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return Format.R32G32B32A32_SInt;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return Format.R32G32B32A32_Float;

                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgba_UNorm:
                    return Format.BC1_UNorm;
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                    return Format.BC1_UNorm_SRgb;
                case PixelFormat.BC2_UNorm:
                    return Format.BC2_UNorm;
                case PixelFormat.BC2_UNorm_SRgb:
                    return Format.BC2_UNorm_SRgb;
                case PixelFormat.BC3_UNorm:
                    return Format.BC3_UNorm;
                case PixelFormat.BC3_UNorm_SRgb:
                    return Format.BC3_UNorm_SRgb;
                case PixelFormat.BC4_UNorm:
                    return Format.BC4_UNorm;
                case PixelFormat.BC4_SNorm:
                    return Format.BC4_SNorm;
                case PixelFormat.BC5_UNorm:
                    return Format.BC5_UNorm;
                case PixelFormat.BC5_SNorm:
                    return Format.BC5_SNorm;
                case PixelFormat.BC7_UNorm:
                    return Format.BC7_UNorm;
                case PixelFormat.BC7_UNorm_SRgb:
                    return Format.BC7_UNorm_SRgb;

                case PixelFormat.D24_UNorm_S8_UInt:
                    Debug.Assert(depthFormat);
                    return Format.R24G8_Typeless;
                case PixelFormat.D32_Float_S8_UInt:
                    Debug.Assert(depthFormat);
                    return Format.R32G8X24_Typeless;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return Format.R10G10B10A2_UNorm;
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return Format.R10G10B10A2_UInt;
                case PixelFormat.R11_G11_B10_Float:
                    return Format.R11G11B10_Float;

                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    throw new VeldridException("ETC2 formats are not supported on Direct3D 11.");

                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal static Format GetTypelessFormat(Format format)
        {
            switch (format)
            {
                case Format.R32G32B32A32_Typeless:
                case Format.R32G32B32A32_Float:
                case Format.R32G32B32A32_UInt:
                case Format.R32G32B32A32_SInt:
                    return Format.R32G32B32A32_Typeless;
                case Format.R32G32B32_Typeless:
                case Format.R32G32B32_Float:
                case Format.R32G32B32_UInt:
                case Format.R32G32B32_SInt:
                    return Format.R32G32B32_Typeless;
                case Format.R16G16B16A16_Typeless:
                case Format.R16G16B16A16_Float:
                case Format.R16G16B16A16_UNorm:
                case Format.R16G16B16A16_UInt:
                case Format.R16G16B16A16_SNorm:
                case Format.R16G16B16A16_SInt:
                    return Format.R16G16B16A16_Typeless;
                case Format.R32G32_Typeless:
                case Format.R32G32_Float:
                case Format.R32G32_UInt:
                case Format.R32G32_SInt:
                    return Format.R32G32_Typeless;
                case Format.R10G10B10A2_Typeless:
                case Format.R10G10B10A2_UNorm:
                case Format.R10G10B10A2_UInt:
                    return Format.R10G10B10A2_Typeless;
                case Format.R8G8B8A8_Typeless:
                case Format.R8G8B8A8_UNorm:
                case Format.R8G8B8A8_UNorm_SRgb:
                case Format.R8G8B8A8_UInt:
                case Format.R8G8B8A8_SNorm:
                case Format.R8G8B8A8_SInt:
                    return Format.R8G8B8A8_Typeless;
                case Format.R16G16_Typeless:
                case Format.R16G16_Float:
                case Format.R16G16_UNorm:
                case Format.R16G16_UInt:
                case Format.R16G16_SNorm:
                case Format.R16G16_SInt:
                    return Format.R16G16_Typeless;
                case Format.R32_Typeless:
                case Format.D32_Float:
                case Format.R32_Float:
                case Format.R32_UInt:
                case Format.R32_SInt:
                    return Format.R32_Typeless;
                case Format.R24G8_Typeless:
                case Format.D24_UNorm_S8_UInt:
                case Format.R24_UNorm_X8_Typeless:
                case Format.X24_Typeless_G8_UInt:
                    return Format.R24G8_Typeless;
                case Format.R8G8_Typeless:
                case Format.R8G8_UNorm:
                case Format.R8G8_UInt:
                case Format.R8G8_SNorm:
                case Format.R8G8_SInt:
                    return Format.R8G8_Typeless;
                case Format.R16_Typeless:
                case Format.R16_Float:
                case Format.D16_UNorm:
                case Format.R16_UNorm:
                case Format.R16_UInt:
                case Format.R16_SNorm:
                case Format.R16_SInt:
                    return Format.R16_Typeless;
                case Format.R8_Typeless:
                case Format.R8_UNorm:
                case Format.R8_UInt:
                case Format.R8_SNorm:
                case Format.R8_SInt:
                case Format.A8_UNorm:
                    return Format.R8_Typeless;
                case Format.BC1_Typeless:
                case Format.BC1_UNorm:
                case Format.BC1_UNorm_SRgb:
                    return Format.BC1_Typeless;
                case Format.BC2_Typeless:
                case Format.BC2_UNorm:
                case Format.BC2_UNorm_SRgb:
                    return Format.BC2_Typeless;
                case Format.BC3_Typeless:
                case Format.BC3_UNorm:
                case Format.BC3_UNorm_SRgb:
                    return Format.BC3_Typeless;
                case Format.BC4_Typeless:
                case Format.BC4_UNorm:
                case Format.BC4_SNorm:
                    return Format.BC4_Typeless;
                case Format.BC5_Typeless:
                case Format.BC5_UNorm:
                case Format.BC5_SNorm:
                    return Format.BC5_Typeless;
                case Format.B8G8R8A8_Typeless:
                case Format.B8G8R8A8_UNorm:
                case Format.B8G8R8A8_UNorm_SRgb:
                    return Format.B8G8R8A8_Typeless;
                case Format.BC7_Typeless:
                case Format.BC7_UNorm:
                case Format.BC7_UNorm_SRgb:
                    return Format.BC7_Typeless;
                default:
                    return format;
            }
        }

        internal static BindFlags VdToD3D11BindFlags(BufferUsage usage)
        {
            BindFlags flags = BindFlags.None;
            if ((usage & BufferUsage.VertexBuffer) == BufferUsage.VertexBuffer)
            {
                flags |= BindFlags.VertexBuffer;
            }
            if ((usage & BufferUsage.IndexBuffer) == BufferUsage.IndexBuffer)
            {
                flags |= BindFlags.IndexBuffer;
            }
            if ((usage & BufferUsage.UniformBuffer) == BufferUsage.UniformBuffer)
            {
                flags |= BindFlags.ConstantBuffer;
            }
            if ((usage & BufferUsage.StructuredBufferReadOnly) == BufferUsage.StructuredBufferReadOnly
                || (usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite)
            {
                flags |= BindFlags.ShaderResource;
            }
            if ((usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite)
            {
                flags |= BindFlags.UnorderedAccess;
            }

            return flags;
        }

        internal static TextureUsage GetVdUsage(BindFlags bindFlags, CpuAccessFlags cpuFlags, ResourceOptionFlags optionFlags)
        {
            TextureUsage usage = 0;
            if ((bindFlags & BindFlags.RenderTarget) != 0)
            {
                usage |= TextureUsage.RenderTarget;
            }
            if ((bindFlags & BindFlags.DepthStencil) != 0)
            {
                usage |= TextureUsage.DepthStencil;
            }
            if ((bindFlags & BindFlags.ShaderResource) != 0)
            {
                usage |= TextureUsage.Sampled;
            }
            if ((bindFlags & BindFlags.UnorderedAccess) != 0)
            {
                usage |= TextureUsage.Storage;
            }

            if ((optionFlags & ResourceOptionFlags.TextureCube) != 0)
            {
                usage |= TextureUsage.Cubemap;
            }
            if ((optionFlags & ResourceOptionFlags.GenerateMips) != 0)
            {
                usage |= TextureUsage.GenerateMipmaps;
            }

            return usage;
        }

        internal static bool IsUnsupportedFormat(PixelFormat format)
        {
            return format == PixelFormat.ETC2_R8_G8_B8_UNorm
                || format == PixelFormat.ETC2_R8_G8_B8_A1_UNorm
                || format == PixelFormat.ETC2_R8_G8_B8_A8_UNorm;
        }

        internal static Format GetViewFormat(Format format)
        {
            return format switch
            {
                Format.R16_Typeless => Format.R16_UNorm,
                Format.R32_Typeless => Format.R32_Float,
                Format.R32G8X24_Typeless => Format.R32_Float_X8X24_Typeless,
                Format.R24G8_Typeless => Format.R24_UNorm_X8_Typeless,
                _ => format,
            };
        }

        internal static Blend VdToD3D11Blend(BlendFactor factor)
        {
            return factor switch
            {
                BlendFactor.Zero => Blend.Zero,
                BlendFactor.One => Blend.One,
                BlendFactor.SourceAlpha => Blend.SourceAlpha,
                BlendFactor.InverseSourceAlpha => Blend.InverseSourceAlpha,
                BlendFactor.DestinationAlpha => Blend.DestinationAlpha,
                BlendFactor.InverseDestinationAlpha => Blend.InverseDestinationAlpha,
                BlendFactor.SourceColor => Blend.SourceColor,
                BlendFactor.InverseSourceColor => Blend.InverseSourceColor,
                BlendFactor.DestinationColor => Blend.DestinationColor,
                BlendFactor.InverseDestinationColor => Blend.InverseDestinationColor,
                BlendFactor.BlendFactor => Blend.BlendFactor,
                BlendFactor.InverseBlendFactor => Blend.InverseBlendFactor,
                _ => throw Illegal.Value<BlendFactor>(),
            };
        }

        internal static Format ToDxgiFormat(IndexFormat format)
        {
            return format switch
            {
                IndexFormat.UInt16 => Format.R16_UInt,
                IndexFormat.UInt32 => Format.R32_UInt,
                _ => throw Illegal.Value<IndexFormat>(),
            };
        }

        internal static Vortice.Direct3D11.StencilOperation VdToD3D11StencilOperation(StencilOperation op)
        {
            return op switch
            {
                StencilOperation.Keep => Vortice.Direct3D11.StencilOperation.Keep,
                StencilOperation.Zero => Vortice.Direct3D11.StencilOperation.Zero,
                StencilOperation.Replace => Vortice.Direct3D11.StencilOperation.Replace,
                StencilOperation.IncrementAndClamp => Vortice.Direct3D11.StencilOperation.IncrementSaturate,
                StencilOperation.DecrementAndClamp => Vortice.Direct3D11.StencilOperation.DecrementSaturate,
                StencilOperation.Invert => Vortice.Direct3D11.StencilOperation.Invert,
                StencilOperation.IncrementAndWrap => Vortice.Direct3D11.StencilOperation.Increment,
                StencilOperation.DecrementAndWrap => Vortice.Direct3D11.StencilOperation.Decrement,
                _ => throw Illegal.Value<StencilOperation>(),
            };
        }

        internal static PixelFormat ToVdFormat(Format format)
        {
            return format switch
            {
                Format.R8_UNorm => PixelFormat.R8_UNorm,
                Format.R8_SNorm => PixelFormat.R8_SNorm,
                Format.R8_UInt => PixelFormat.R8_UInt,
                Format.R8_SInt => PixelFormat.R8_SInt,
                Format.R16_UNorm or Format.D16_UNorm => PixelFormat.R16_UNorm,
                Format.R16_SNorm => PixelFormat.R16_SNorm,
                Format.R16_UInt => PixelFormat.R16_UInt,
                Format.R16_SInt => PixelFormat.R16_SInt,
                Format.R16_Float => PixelFormat.R16_Float,
                Format.R32_UInt => PixelFormat.R32_UInt,
                Format.R32_SInt => PixelFormat.R32_SInt,
                Format.R32_Float or Format.D32_Float => PixelFormat.R32_Float,
                Format.R8G8_UNorm => PixelFormat.R8_G8_UNorm,
                Format.R8G8_SNorm => PixelFormat.R8_G8_SNorm,
                Format.R8G8_UInt => PixelFormat.R8_G8_UInt,
                Format.R8G8_SInt => PixelFormat.R8_G8_SInt,
                Format.R16G16_UNorm => PixelFormat.R16_G16_UNorm,
                Format.R16G16_SNorm => PixelFormat.R16_G16_SNorm,
                Format.R16G16_UInt => PixelFormat.R16_G16_UInt,
                Format.R16G16_SInt => PixelFormat.R16_G16_SInt,
                Format.R16G16_Float => PixelFormat.R16_G16_Float,
                Format.R32G32_UInt => PixelFormat.R32_G32_UInt,
                Format.R32G32_SInt => PixelFormat.R32_G32_SInt,
                Format.R32G32_Float => PixelFormat.R32_G32_Float,
                Format.R8G8B8A8_UNorm => PixelFormat.R8_G8_B8_A8_UNorm,
                Format.R8G8B8A8_UNorm_SRgb => PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                Format.B8G8R8A8_UNorm => PixelFormat.B8_G8_R8_A8_UNorm,
                Format.B8G8R8A8_UNorm_SRgb => PixelFormat.B8_G8_R8_A8_UNorm_SRgb,
                Format.R8G8B8A8_SNorm => PixelFormat.R8_G8_B8_A8_SNorm,
                Format.R8G8B8A8_UInt => PixelFormat.R8_G8_B8_A8_UInt,
                Format.R8G8B8A8_SInt => PixelFormat.R8_G8_B8_A8_SInt,
                Format.R16G16B16A16_UNorm => PixelFormat.R16_G16_B16_A16_UNorm,
                Format.R16G16B16A16_SNorm => PixelFormat.R16_G16_B16_A16_SNorm,
                Format.R16G16B16A16_UInt => PixelFormat.R16_G16_B16_A16_UInt,
                Format.R16G16B16A16_SInt => PixelFormat.R16_G16_B16_A16_SInt,
                Format.R16G16B16A16_Float => PixelFormat.R16_G16_B16_A16_Float,
                Format.R32G32B32A32_UInt => PixelFormat.R32_G32_B32_A32_UInt,
                Format.R32G32B32A32_SInt => PixelFormat.R32_G32_B32_A32_SInt,
                Format.R32G32B32A32_Float => PixelFormat.R32_G32_B32_A32_Float,
                Format.BC1_UNorm or Format.BC1_Typeless => PixelFormat.BC1_Rgba_UNorm,
                Format.BC2_UNorm => PixelFormat.BC2_UNorm,
                Format.BC3_UNorm => PixelFormat.BC3_UNorm,
                Format.BC4_UNorm => PixelFormat.BC4_UNorm,
                Format.BC4_SNorm => PixelFormat.BC4_SNorm,
                Format.BC5_UNorm => PixelFormat.BC5_UNorm,
                Format.BC5_SNorm => PixelFormat.BC5_SNorm,
                Format.BC7_UNorm => PixelFormat.BC7_UNorm,
                Format.D24_UNorm_S8_UInt => PixelFormat.D24_UNorm_S8_UInt,
                Format.D32_Float_S8X24_UInt => PixelFormat.D32_Float_S8_UInt,
                Format.R10G10B10A2_UInt => PixelFormat.R10_G10_B10_A2_UInt,
                Format.R10G10B10A2_UNorm => PixelFormat.R10_G10_B10_A2_UNorm,
                Format.R11G11B10_Float => PixelFormat.R11_G11_B10_Float,
                _ => throw Illegal.Value<PixelFormat>(),
            };
        }

        internal static BlendOperation VdToD3D11BlendOperation(BlendFunction function)
        {
            return function switch
            {
                BlendFunction.Add => BlendOperation.Add,
                BlendFunction.Subtract => BlendOperation.Subtract,
                BlendFunction.ReverseSubtract => BlendOperation.ReverseSubtract,
                BlendFunction.Minimum => BlendOperation.Min,
                BlendFunction.Maximum => BlendOperation.Max,
                _ => throw Illegal.Value<BlendFunction>(),
            };
        }

        internal static ColorWriteEnable VdToD3D11ColorWriteEnable(ColorWriteMask mask)
        {
            ColorWriteEnable enable = ColorWriteEnable.None;

            if ((mask & ColorWriteMask.Red) == ColorWriteMask.Red)
                enable |= ColorWriteEnable.Red;
            if ((mask & ColorWriteMask.Green) == ColorWriteMask.Green)
                enable |= ColorWriteEnable.Green;
            if ((mask & ColorWriteMask.Blue) == ColorWriteMask.Blue)
                enable |= ColorWriteEnable.Blue;
            if ((mask & ColorWriteMask.Alpha) == ColorWriteMask.Alpha)
                enable |= ColorWriteEnable.Alpha;

            return enable;
        }

        internal static Filter ToD3D11Filter(SamplerFilter filter, bool isComparison)
        {
            if (isComparison)
            {
                return filter switch
                {
                    SamplerFilter.MinPoint_MagPoint_MipPoint => Filter.ComparisonMinMagMipPoint,
                    SamplerFilter.MinPoint_MagPoint_MipLinear => Filter.ComparisonMinMagPointMipLinear,
                    SamplerFilter.MinPoint_MagLinear_MipPoint => Filter.ComparisonMinPointMagLinearMipPoint,
                    SamplerFilter.MinPoint_MagLinear_MipLinear => Filter.ComparisonMinPointMagMipLinear,
                    SamplerFilter.MinLinear_MagPoint_MipPoint => Filter.ComparisonMinLinearMagMipPoint,
                    SamplerFilter.MinLinear_MagPoint_MipLinear => Filter.ComparisonMinLinearMagPointMipLinear,
                    SamplerFilter.MinLinear_MagLinear_MipPoint => Filter.ComparisonMinMagLinearMipPoint,
                    SamplerFilter.MinLinear_MagLinear_MipLinear => Filter.ComparisonMinMagMipLinear,
                    SamplerFilter.Anisotropic => Filter.ComparisonAnisotropic,
                    _ => throw Illegal.Value<SamplerFilter>(),
                };
            }
            else
            {
                return filter switch
                {
                    SamplerFilter.MinPoint_MagPoint_MipPoint => Filter.MinMagMipPoint,
                    SamplerFilter.MinPoint_MagPoint_MipLinear => Filter.MinMagPointMipLinear,
                    SamplerFilter.MinPoint_MagLinear_MipPoint => Filter.MinPointMagLinearMipPoint,
                    SamplerFilter.MinPoint_MagLinear_MipLinear => Filter.MinPointMagMipLinear,
                    SamplerFilter.MinLinear_MagPoint_MipPoint => Filter.MinLinearMagMipPoint,
                    SamplerFilter.MinLinear_MagPoint_MipLinear => Filter.MinLinearMagPointMipLinear,
                    SamplerFilter.MinLinear_MagLinear_MipPoint => Filter.MinMagLinearMipPoint,
                    SamplerFilter.MinLinear_MagLinear_MipLinear => Filter.MinMagMipLinear,
                    SamplerFilter.Anisotropic => Filter.Anisotropic,
                    _ => throw Illegal.Value<SamplerFilter>(),
                };
            }
        }

        internal static Vortice.Direct3D11.MapMode VdToD3D11MapMode(bool isDynamic, MapMode mode)
        {
            return mode switch
            {
                MapMode.Read => Vortice.Direct3D11.MapMode.Read,
                MapMode.Write => isDynamic ? Vortice.Direct3D11.MapMode.WriteDiscard : Vortice.Direct3D11.MapMode.Write,
                MapMode.ReadWrite => Vortice.Direct3D11.MapMode.ReadWrite,
                _ => throw Illegal.Value<MapMode>(),
            };
        }

        internal static Vortice.Direct3D.PrimitiveTopology VdToD3D11PrimitiveTopology(PrimitiveTopology primitiveTopology)
        {
            return primitiveTopology switch
            {
                PrimitiveTopology.TriangleList => Vortice.Direct3D.PrimitiveTopology.TriangleList,
                PrimitiveTopology.TriangleStrip => Vortice.Direct3D.PrimitiveTopology.TriangleStrip,
                PrimitiveTopology.LineList => Vortice.Direct3D.PrimitiveTopology.LineList,
                PrimitiveTopology.LineStrip => Vortice.Direct3D.PrimitiveTopology.LineStrip,
                PrimitiveTopology.PointList => Vortice.Direct3D.PrimitiveTopology.PointList,
                _ => throw Illegal.Value<PrimitiveTopology>(),
            };
        }

        internal static FillMode VdToD3D11FillMode(PolygonFillMode fillMode)
        {
            return fillMode switch
            {
                PolygonFillMode.Solid => FillMode.Solid,
                PolygonFillMode.Wireframe => FillMode.Wireframe,
                _ => throw Illegal.Value<PolygonFillMode>(),
            };
        }

        internal static CullMode VdToD3D11CullMode(FaceCullMode cullingMode)
        {
            return cullingMode switch
            {
                FaceCullMode.Back => CullMode.Back,
                FaceCullMode.Front => CullMode.Front,
                FaceCullMode.None => CullMode.None,
                _ => throw Illegal.Value<FaceCullMode>(),
            };
        }

        internal static Format ToDxgiFormat(VertexElementFormat format)
        {
            return format switch
            {
                VertexElementFormat.Float1 => Format.R32_Float,
                VertexElementFormat.Float2 => Format.R32G32_Float,
                VertexElementFormat.Float3 => Format.R32G32B32_Float,
                VertexElementFormat.Float4 => Format.R32G32B32A32_Float,
                VertexElementFormat.Byte2_Norm => Format.R8G8_UNorm,
                VertexElementFormat.Byte2 => Format.R8G8_UInt,
                VertexElementFormat.Byte4_Norm => Format.R8G8B8A8_UNorm,
                VertexElementFormat.Byte4 => Format.R8G8B8A8_UInt,
                VertexElementFormat.SByte2_Norm => Format.R8G8_SNorm,
                VertexElementFormat.SByte2 => Format.R8G8_SInt,
                VertexElementFormat.SByte4_Norm => Format.R8G8B8A8_SNorm,
                VertexElementFormat.SByte4 => Format.R8G8B8A8_SInt,
                VertexElementFormat.UShort2_Norm => Format.R16G16_UNorm,
                VertexElementFormat.UShort2 => Format.R16G16_UInt,
                VertexElementFormat.UShort4_Norm => Format.R16G16B16A16_UNorm,
                VertexElementFormat.UShort4 => Format.R16G16B16A16_UInt,
                VertexElementFormat.Short2_Norm => Format.R16G16_SNorm,
                VertexElementFormat.Short2 => Format.R16G16_SInt,
                VertexElementFormat.Short4_Norm => Format.R16G16B16A16_SNorm,
                VertexElementFormat.Short4 => Format.R16G16B16A16_SInt,
                VertexElementFormat.UInt1 => Format.R32_UInt,
                VertexElementFormat.UInt2 => Format.R32G32_UInt,
                VertexElementFormat.UInt3 => Format.R32G32B32_UInt,
                VertexElementFormat.UInt4 => Format.R32G32B32A32_UInt,
                VertexElementFormat.Int1 => Format.R32_SInt,
                VertexElementFormat.Int2 => Format.R32G32_SInt,
                VertexElementFormat.Int3 => Format.R32G32B32_SInt,
                VertexElementFormat.Int4 => Format.R32G32B32A32_SInt,
                VertexElementFormat.Half1 => Format.R16_Float,
                VertexElementFormat.Half2 => Format.R16G16_Float,
                VertexElementFormat.Half4 => Format.R16G16B16A16_Float,
                _ => throw Illegal.Value<VertexElementFormat>(),
            };
        }

        internal static ComparisonFunction VdToD3D11ComparisonFunc(ComparisonKind comparisonKind)
        {
            return comparisonKind switch
            {
                ComparisonKind.Never => ComparisonFunction.Never,
                ComparisonKind.Less => ComparisonFunction.Less,
                ComparisonKind.Equal => ComparisonFunction.Equal,
                ComparisonKind.LessEqual => ComparisonFunction.LessEqual,
                ComparisonKind.Greater => ComparisonFunction.Greater,
                ComparisonKind.NotEqual => ComparisonFunction.NotEqual,
                ComparisonKind.GreaterEqual => ComparisonFunction.GreaterEqual,
                ComparisonKind.Always => ComparisonFunction.Always,
                _ => throw Illegal.Value<ComparisonKind>(),
            };
        }

        internal static TextureAddressMode VdToD3D11AddressMode(SamplerAddressMode mode)
        {
            return mode switch
            {
                SamplerAddressMode.Wrap => TextureAddressMode.Wrap,
                SamplerAddressMode.Mirror => TextureAddressMode.Mirror,
                SamplerAddressMode.Clamp => TextureAddressMode.Clamp,
                SamplerAddressMode.Border => TextureAddressMode.Border,
                _ => throw Illegal.Value<SamplerAddressMode>(),
            };
        }

        internal static Format GetDepthFormat(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.R32_Float => Format.D32_Float,
                PixelFormat.R16_UNorm => Format.D16_UNorm,
                PixelFormat.D24_UNorm_S8_UInt => Format.D24_UNorm_S8_UInt,
                PixelFormat.D32_Float_S8_UInt => Format.D32_Float_S8X24_UInt,
                _ => throw new VeldridException("Invalid depth texture format: " + format),
            };
        }
    }
}
