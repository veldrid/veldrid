using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Veldrid.D3D11
{
    internal static class D3D11Formats
    {
        internal static Format ToDxgiFormat(PixelFormat format, bool depthFormat)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return Format.R8G8B8A8_UNorm;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return Format.B8G8R8A8_UNorm;
                case PixelFormat.R8_UNorm:
                    return Format.R8_UNorm;
                case PixelFormat.R16_UNorm:
                    return depthFormat ? Format.R16_Typeless : Format.R16_UNorm;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return Format.R32G32B32A32_Float;
                case PixelFormat.R32_Float:
                    return depthFormat ? Format.D32_Float : Format.R32_Float;
                default:
                    throw Illegal.Value<PixelFormat>();
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

        internal static Format GetViewFormat(Format format)
        {
            switch (format)
            {
                case Format.R16_Typeless:
                    return Format.R16_UNorm;
                default:
                    return format;
            }
        }

        internal static BlendOption VdToD3D11BlendOption(BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Zero:
                    return BlendOption.Zero;
                case BlendFactor.One:
                    return BlendOption.One;
                case BlendFactor.SourceAlpha:
                    return BlendOption.SourceAlpha;
                case BlendFactor.InverseSourceAlpha:
                    return BlendOption.InverseSourceAlpha;
                case BlendFactor.DestinationAlpha:
                    return BlendOption.DestinationAlpha;
                case BlendFactor.InverseDestinationAlpha:
                    return BlendOption.InverseDestinationAlpha;
                case BlendFactor.SourceColor:
                    return BlendOption.SourceColor;
                case BlendFactor.InverseSourceColor:
                    return BlendOption.InverseSourceColor;
                case BlendFactor.DestinationColor:
                    return BlendOption.DestinationColor;
                case BlendFactor.InverseDestinationColor:
                    return BlendOption.InverseDestinationColor;
                case BlendFactor.BlendFactor:
                    return BlendOption.BlendFactor;
                case BlendFactor.InverseBlendFactor:
                    return BlendOption.BlendFactor;
                default:
                    throw Illegal.Value<BlendFactor>();
            }
        }

        internal static Format ToDxgiFormat(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt16:
                    return Format.R16_UInt;
                case IndexFormat.UInt32:
                    return Format.R32_UInt;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        internal static PixelFormat ToVdFormat(Format format)
        {
            switch (format)
            {
                case Format.R8G8B8A8_UNorm:
                    return PixelFormat.R8_G8_B8_A8_UNorm;
                case Format.B8G8R8A8_UNorm:
                    return PixelFormat.B8_G8_R8_A8_UNorm;
                case Format.R8_UNorm:
                    return PixelFormat.R8_UNorm;
                case Format.R16_UNorm:
                case Format.D16_UNorm:
                    return PixelFormat.R16_UNorm;
                case Format.R32G32B32A32_Float:
                    return PixelFormat.R32_G32_B32_A32_Float;
                case Format.R32_Float:
                    return PixelFormat.R32_Float;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static TextureSampleCount ToVdSampleCount(SampleDescription sampleDescription)
        {
            switch (sampleDescription.Count)
            {
                case 1: return TextureSampleCount.Count1;
                case 2: return TextureSampleCount.Count2;
                case 4: return TextureSampleCount.Count4;
                case 8: return TextureSampleCount.Count8;
                case 16: return TextureSampleCount.Count16;
                case 32: return TextureSampleCount.Count32;
                default: throw new VeldridException("Unsupported multisample count: " + sampleDescription.Count);
            }
        }

        internal static BlendOperation VdToD3D11BlendOperation(BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return BlendOperation.Add;
                case BlendFunction.Subtract:
                    return BlendOperation.Subtract;
                case BlendFunction.ReverseSubtract:
                    return BlendOperation.ReverseSubtract;
                case BlendFunction.Minimum:
                    return BlendOperation.Minimum;
                case BlendFunction.Maximum:
                    return BlendOperation.Maximum;
                default:
                    throw Illegal.Value<BlendFunction>();
            }
        }

        internal static Filter ToD3D11Filter(SamplerFilter filter, bool isComparison)
        {
            switch (filter)
            {
                case SamplerFilter.MinPoint_MagPoint_MipPoint:
                    return isComparison ? Filter.ComparisonMinMagMipPoint : Filter.MinMagMipPoint;
                case SamplerFilter.MinPoint_MagPoint_MipLinear:
                    return isComparison ? Filter.ComparisonMinMagPointMipLinear : Filter.MinMagPointMipLinear;
                case SamplerFilter.MinPoint_MagLinear_MipPoint:
                    return isComparison ? Filter.ComparisonMinPointMagLinearMipPoint : Filter.MinPointMagLinearMipPoint;
                case SamplerFilter.MinPoint_MagLinear_MipLinear:
                    return isComparison ? Filter.ComparisonMinPointMagMipLinear : Filter.MinPointMagMipLinear;
                case SamplerFilter.MinLinear_MagPoint_MipPoint:
                    return isComparison ? Filter.ComparisonMinLinearMagMipPoint : Filter.MinLinearMagMipPoint;
                case SamplerFilter.MinLinear_MagPoint_MipLinear:
                    return isComparison ? Filter.ComparisonMinLinearMagPointMipLinear : Filter.MinLinearMagPointMipLinear;
                case SamplerFilter.MinLinear_MagLinear_MipPoint:
                    return isComparison ? Filter.ComparisonMinMagLinearMipPoint : Filter.MinMagLinearMipPoint;
                case SamplerFilter.MinLinear_MagLinear_MipLinear:
                    return isComparison ? Filter.ComparisonMinMagMipLinear : Filter.MinMagMipLinear;
                case SamplerFilter.Anisotropic:
                    return isComparison ? Filter.ComparisonAnisotropic : Filter.Anisotropic;
                default:
                    throw Illegal.Value<SamplerFilter>();
            }
        }

        internal static SharpDX.Direct3D11.MapMode VdToD3D11MapMode(bool isDynamic, MapMode mode)
        {
            switch (mode)
            {
                case MapMode.Read:
                    return SharpDX.Direct3D11.MapMode.Read;
                case MapMode.Write:
                    return isDynamic ? SharpDX.Direct3D11.MapMode.WriteDiscard : SharpDX.Direct3D11.MapMode.Write;
                case MapMode.ReadWrite:
                    return SharpDX.Direct3D11.MapMode.ReadWrite;
                default:
                    throw Illegal.Value<MapMode>();
            }
        }

        internal static SharpDX.Direct3D.PrimitiveTopology VdToD3D11PrimitiveTopology(PrimitiveTopology primitiveTopology)
        {
            switch (primitiveTopology)
            {
                case PrimitiveTopology.TriangleList:
                    return SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                case PrimitiveTopology.TriangleStrip:
                    return SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;
                case PrimitiveTopology.LineList:
                    return SharpDX.Direct3D.PrimitiveTopology.LineList;
                case PrimitiveTopology.LineStrip:
                    return SharpDX.Direct3D.PrimitiveTopology.LineStrip;
                case PrimitiveTopology.PointList:
                    return SharpDX.Direct3D.PrimitiveTopology.PointList;
                default:
                    throw Illegal.Value<PrimitiveTopology>();
            }
        }

        internal static FillMode VdToD3D11FillMode(PolygonFillMode fillMode)
        {
            switch (fillMode)
            {
                case PolygonFillMode.Solid:
                    return FillMode.Solid;
                case PolygonFillMode.Wireframe:
                    return FillMode.Wireframe;
                default:
                    throw Illegal.Value<PolygonFillMode>();
            }
        }

        internal static CullMode VdToD3D11CullMode(FaceCullMode cullingMode)
        {
            switch (cullingMode)
            {
                case FaceCullMode.Back:
                    return CullMode.Back;
                case FaceCullMode.Front:
                    return CullMode.Front;
                case FaceCullMode.None:
                    return CullMode.None;
                default:
                    throw Illegal.Value<FaceCullMode>();
            }
        }

        internal static Format ToDxgiFormat(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                    return Format.R32_Float;
                case VertexElementFormat.Float2:
                    return Format.R32G32_Float;
                case VertexElementFormat.Float3:
                    return Format.R32G32B32_Float;
                case VertexElementFormat.Float4:
                    return Format.R32G32B32A32_Float;
                case VertexElementFormat.Byte1:
                    return Format.R8_UNorm;
                case VertexElementFormat.Byte2:
                    return Format.R8G8_UNorm;
                case VertexElementFormat.Byte4:
                    return Format.R8G8B8A8_UNorm;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        internal static Comparison VdToD3D11DepthComparison(DepthComparisonKind comparisonKind)
        {
            switch (comparisonKind)
            {
                case DepthComparisonKind.Never:
                    return Comparison.Never;
                case DepthComparisonKind.Less:
                    return Comparison.Less;
                case DepthComparisonKind.Equal:
                    return Comparison.Equal;
                case DepthComparisonKind.LessEqual:
                    return Comparison.LessEqual;
                case DepthComparisonKind.Greater:
                    return Comparison.Greater;
                case DepthComparisonKind.NotEqual:
                    return Comparison.NotEqual;
                case DepthComparisonKind.GreaterEqual:
                    return Comparison.GreaterEqual;
                case DepthComparisonKind.Always:
                    return Comparison.Always;
                default:
                    throw Illegal.Value<DepthComparisonKind>();
            }
        }

        internal static TextureAddressMode VdToD3D11AddressMode(SamplerAddressMode mode)
        {
            switch (mode)
            {
                case SamplerAddressMode.Wrap:
                    return TextureAddressMode.Wrap;
                case SamplerAddressMode.Mirror:
                    return TextureAddressMode.Mirror;
                case SamplerAddressMode.Clamp:
                    return TextureAddressMode.Clamp;
                case SamplerAddressMode.Border:
                    return TextureAddressMode.Border;
                default:
                    throw Illegal.Value<SamplerAddressMode>();
            }
        }

        internal static Format GetDepthFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_Float:
                    return Format.D32_Float;
                case PixelFormat.R16_UNorm:
                    return Format.D16_UNorm;
                default:
                    throw new VeldridException("Invalid depth texture format: " + format);
            }
        }
    }
}