using System;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DFormats
    {
        internal static Format VeldridToD3DPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return Format.R32G32B32A32_Float;
                case PixelFormat.R8_UInt:
                    return Format.R8_UInt;
                case PixelFormat.Alpha_UInt8:
                    return Format.A8_UNorm;
                case PixelFormat.R8_G8_B8_A8:
                    return Format.R8G8B8A8_UNorm;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static SamplerAddressMode D3DToVeldridSamplerAddressMode(TextureAddressMode mode)
        {
            switch (mode)
            {
                case TextureAddressMode.Wrap:
                    return SamplerAddressMode.Wrap;
                case TextureAddressMode.Mirror:
                    return SamplerAddressMode.Mirror;
                case TextureAddressMode.Clamp:
                    return SamplerAddressMode.Clamp;
                case TextureAddressMode.Border:
                    return SamplerAddressMode.Border;
                default:
                    throw Illegal.Value<TextureAddressMode>();
            }
        }

        internal static TextureAddressMode VeldridToD3DSamplerAddressMode(SamplerAddressMode mode)
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

        internal static SamplerFilter D3DToVeldridSamplerFilter(Filter filter)
        {
            switch (filter)
            {
                case Filter.MinMagMipPoint:
                    return SamplerFilter.MinMagMipPoint;
                case Filter.MinMagPointMipLinear:
                    return SamplerFilter.MinMagPointMipLinear;
                case Filter.MinPointMagLinearMipPoint:
                    return SamplerFilter.MinPointMagLinearMipPoint;
                case Filter.MinPointMagMipLinear:
                    return SamplerFilter.MinPointMagMipLinear;
                case Filter.MinLinearMagMipPoint:
                    return SamplerFilter.MinLinearMagMipPoint;
                case Filter.MinLinearMagPointMipLinear:
                    return SamplerFilter.MinLinearMagPointMipLinear;
                case Filter.MinMagLinearMipPoint:
                    return SamplerFilter.MinMagLinearMipPoint;
                case Filter.MinMagMipLinear:
                    return SamplerFilter.MinMagMipLinear;
                case Filter.Anisotropic:
                    return SamplerFilter.Anisotropic;
                case Filter.ComparisonMinMagMipPoint:
                    return SamplerFilter.ComparisonMinMagMipPoint;
                case Filter.ComparisonMinMagPointMipLinear:
                    return SamplerFilter.ComparisonMinMagPointMipLinear;
                case Filter.ComparisonMinPointMagLinearMipPoint:
                    return SamplerFilter.ComparisonMinPointMagLinearMipPoint;
                case Filter.ComparisonMinPointMagMipLinear:
                    return SamplerFilter.ComparisonMinPointMagMipLinear;
                case Filter.ComparisonMinLinearMagMipPoint:
                    return SamplerFilter.ComparisonMinLinearMagMipPoint;
                case Filter.ComparisonMinLinearMagPointMipLinear:
                    return SamplerFilter.ComparisonMinLinearMagPointMipLinear;
                case Filter.ComparisonMinMagLinearMipPoint:
                    return SamplerFilter.ComparisonMinMagLinearMipPoint;
                case Filter.ComparisonMinMagMipLinear:
                    return SamplerFilter.ComparisonMinMagMipLinear;
                case Filter.ComparisonAnisotropic:
                    return SamplerFilter.ComparisonAnisotropic;
                default:
                    throw Illegal.Value<Filter>();
            }
        }

        internal static Filter VeldridToD3DSamplerFilter(SamplerFilter filter)
        {
            switch (filter)
            {
                case SamplerFilter.MinMagMipPoint:
                    return Filter.MinMagMipPoint;
                case SamplerFilter.MinMagPointMipLinear:
                    return Filter.MinMagPointMipLinear;
                case SamplerFilter.MinPointMagLinearMipPoint:
                    return Filter.MinPointMagLinearMipPoint;
                case SamplerFilter.MinPointMagMipLinear:
                    return Filter.MinPointMagMipLinear;
                case SamplerFilter.MinLinearMagMipPoint:
                    return Filter.MinLinearMagMipPoint;
                case SamplerFilter.MinLinearMagPointMipLinear:
                    return Filter.MinLinearMagPointMipLinear;
                case SamplerFilter.MinMagLinearMipPoint:
                    return Filter.MinMagLinearMipPoint;
                case SamplerFilter.MinMagMipLinear:
                    return Filter.MinMagMipLinear;
                case SamplerFilter.Anisotropic:
                    return Filter.Anisotropic;
                case SamplerFilter.ComparisonMinMagMipPoint:
                    return Filter.ComparisonMinMagMipPoint;
                case SamplerFilter.ComparisonMinMagPointMipLinear:
                    return Filter.ComparisonMinMagPointMipLinear;
                case SamplerFilter.ComparisonMinPointMagLinearMipPoint:
                    return Filter.ComparisonMinPointMagLinearMipPoint;
                case SamplerFilter.ComparisonMinPointMagMipLinear:
                    return Filter.ComparisonMinPointMagMipLinear;
                case SamplerFilter.ComparisonMinLinearMagMipPoint:
                    return Filter.ComparisonMinLinearMagMipPoint;
                case SamplerFilter.ComparisonMinLinearMagPointMipLinear:
                    return Filter.ComparisonMinLinearMagPointMipLinear;
                case SamplerFilter.ComparisonMinMagLinearMipPoint:
                    return Filter.ComparisonMinMagLinearMipPoint;
                case SamplerFilter.ComparisonMinMagMipLinear:
                    return Filter.ComparisonMinMagMipLinear;
                case SamplerFilter.ComparisonAnisotropic:
                    return Filter.ComparisonAnisotropic;
                default:
                    throw Illegal.Value<SamplerFilter>();
            }
        }


        internal static Format VeldridToD3DIndexFormat(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt32:
                    return Format.R32_UInt;
                case IndexFormat.UInt16:
                    return Format.R16_UInt;
                case IndexFormat.UInt8:
                    return Format.R8_UInt;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        internal static FillMode VeldridToD3DTriangleFillMode(TriangleFillMode fillMode)
        {
            switch (fillMode)
            {
                case TriangleFillMode.Solid:
                    return FillMode.Solid;
                case TriangleFillMode.Wireframe:
                    return FillMode.Wireframe;
                default:
                    throw Illegal.Value<TriangleFillMode>();
            }
        }

        internal static CullMode VeldridToD3DFaceCullingMode(FaceCullingMode cullMode)
        {
            switch (cullMode)
            {
                case FaceCullingMode.Back:
                    return CullMode.Back;
                case FaceCullingMode.Front:
                    return CullMode.Front;
                case FaceCullingMode.None:
                    return CullMode.None;
                default:
                    throw Illegal.Value<FaceCullingMode>();
            }
        }

        public static BlendOption VeldridToD3DBlend(Blend blendFactor)
        {
            switch (blendFactor)
            {
                case Blend.Zero:
                    return BlendOption.Zero;
                case Blend.One:
                    return BlendOption.One;
                case Blend.SourceAlpha:
                    return BlendOption.SourceAlpha;
                case Blend.InverseSourceAlpha:
                    return BlendOption.InverseSourceAlpha;
                case Blend.DestinationAlpha:
                    return BlendOption.DestinationAlpha;
                case Blend.InverseDestinationAlpha:
                    return BlendOption.InverseDestinationAlpha;
                case Blend.SourceColor:
                    return BlendOption.SourceColor;
                case Blend.InverseSourceColor:
                    return BlendOption.InverseSourceColor;
                case Blend.DestinationColor:
                    return BlendOption.DestinationColor;
                case Blend.InverseDestinationColor:
                    return BlendOption.InverseDestinationColor;
                case Blend.BlendFactor:
                    return BlendOption.BlendFactor;
                case Blend.InverseBlendFactor:
                    return BlendOption.InverseBlendFactor;
                default:
                    throw Illegal.Value<Blend>();
            }
        }

        public static BlendOperation VeldridToD3DBlendFunction(BlendFunction function)
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
        public static Comparison VeldridToD3DDepthComparison(DepthComparison dc)
        {
            switch (dc)
            {
                case DepthComparison.Never:
                    return Comparison.Never;
                case DepthComparison.Less:
                    return Comparison.Less;
                case DepthComparison.Equal:
                    return Comparison.Equal;
                case DepthComparison.LessEqual:
                    return Comparison.LessEqual;
                case DepthComparison.Greater:
                    return Comparison.Equal;
                case DepthComparison.NotEqual:
                    return Comparison.NotEqual;
                case DepthComparison.GreaterEqual:
                    return Comparison.GreaterEqual;
                case DepthComparison.Always:
                    return Comparison.Always;
                default:
                    throw Illegal.Value<DepthComparison>();
            }
        }

        internal static DepthComparison D3DToVeldridDepthComparison(Comparison comparisonFunction)
        {
            switch (comparisonFunction)
            {
                case Comparison.Never:
                    return DepthComparison.Never;
                case Comparison.Less:
                    return DepthComparison.Less;
                case Comparison.Equal:
                    return DepthComparison.Equal;
                case Comparison.LessEqual:
                    return DepthComparison.LessEqual;
                case Comparison.Greater:
                    return DepthComparison.Greater;
                case Comparison.NotEqual:
                    return DepthComparison.NotEqual;
                case Comparison.GreaterEqual:
                    return DepthComparison.GreaterEqual;
                case Comparison.Always:
                    return DepthComparison.Always;
                default:
                    throw Illegal.Value<Comparison>();
            }
        }

        public static Format MapFormatForShaderResourceView(Format format)
        {
            switch (format)
            {
                case Format.R16_Typeless:
                    return Format.R16_UNorm;
                default:
                    return format;
            }
        }

        public static SharpDX.Direct3D.PrimitiveTopology VeldridToD3DPrimitiveTopology(PrimitiveTopology primitiveTopology)
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

        public static InputClassification VeldridToD3DVertexElementInputClass(VertexElementInputClass inputClass)
        {
            switch (inputClass)
            {
                case VertexElementInputClass.PerVertex:
                    return InputClassification.PerVertexData;
                case VertexElementInputClass.PerInstance:
                    return InputClassification.PerInstanceData;
                default:
                    throw Illegal.Value<VertexElementInputClass>();
            }

        }

        public static int GetPixelSize(Format format)
        {
            switch (format)
            {
                case Format.R32G32B32A32_Typeless:
                case Format.R32G32B32A32_Float:
                case Format.R32G32B32A32_UInt:
                case Format.R32G32B32A32_SInt:
                    return 16;
                case Format.R32G32B32_Typeless:
                case Format.R32G32B32_Float:
                case Format.R32G32B32_UInt:
                case Format.R32G32B32_SInt:
                    return 12;
                case Format.R16G16B16A16_Typeless:
                case Format.R16G16B16A16_Float:
                case Format.R16G16B16A16_UNorm:
                case Format.R16G16B16A16_UInt:
                case Format.R16G16B16A16_SNorm:
                case Format.R16G16B16A16_SInt:
                    return 8;
                case Format.R32G32_Typeless:
                case Format.R32G32_Float:
                case Format.R32G32_UInt:
                case Format.R32G32_SInt:
                case Format.R32G8X24_Typeless:
                case Format.D32_Float_S8X24_UInt:
                case Format.R32_Float_X8X24_Typeless:
                case Format.X32_Typeless_G8X24_UInt:
                    return 8;
                case Format.R10G10B10A2_Typeless:
                case Format.R10G10B10A2_UNorm:
                case Format.R10G10B10A2_UInt:
                case Format.R11G11B10_Float:
                case Format.R8G8B8A8_Typeless:
                case Format.R8G8B8A8_UNorm:
                case Format.R8G8B8A8_UNorm_SRgb:
                case Format.R8G8B8A8_UInt:
                case Format.R8G8B8A8_SNorm:
                case Format.R8G8B8A8_SInt:
                case Format.R16G16_Typeless:
                case Format.R16G16_Float:
                case Format.R16G16_UNorm:
                case Format.R16G16_UInt:
                case Format.R16G16_SNorm:
                case Format.R16G16_SInt:
                case Format.R32_Typeless:
                case Format.D32_Float:
                case Format.R32_Float:
                case Format.R32_UInt:
                case Format.R32_SInt:
                case Format.R24G8_Typeless:
                case Format.D24_UNorm_S8_UInt:
                case Format.R24_UNorm_X8_Typeless:
                case Format.X24_Typeless_G8_UInt:
                    return 4;
                case Format.R8G8_Typeless:
                case Format.R8G8_UNorm:
                case Format.R8G8_UInt:
                case Format.R8G8_SNorm:
                case Format.R8G8_SInt:
                case Format.R16_Typeless:
                case Format.R16_Float:
                case Format.D16_UNorm:
                case Format.R16_UNorm:
                case Format.R16_UInt:
                case Format.R16_SNorm:
                case Format.R16_SInt:
                    return 2;
                case Format.R8_Typeless:
                case Format.R8_UNorm:
                case Format.R8_UInt:
                case Format.R8_SNorm:
                case Format.R8_SInt:
                case Format.A8_UNorm:
                    return 1;
                case Format.R1_UNorm:
                    return 1;
                case Format.R9G9B9E5_Sharedexp:
                case Format.R8G8_B8G8_UNorm:
                case Format.G8R8_G8B8_UNorm:
                case Format.BC1_Typeless:
                case Format.BC1_UNorm:
                case Format.BC1_UNorm_SRgb:
                case Format.BC2_Typeless:
                case Format.BC2_UNorm:
                case Format.BC2_UNorm_SRgb:
                case Format.BC3_Typeless:
                case Format.BC3_UNorm:
                case Format.BC3_UNorm_SRgb:
                case Format.BC4_Typeless:
                case Format.BC4_UNorm:
                case Format.BC4_SNorm:
                case Format.BC5_Typeless:
                case Format.BC5_UNorm:
                case Format.BC5_SNorm:
                case Format.B5G6R5_UNorm:
                case Format.B5G5R5A1_UNorm:
                case Format.B8G8R8A8_UNorm:
                case Format.B8G8R8X8_UNorm:
                case Format.R10G10B10_Xr_Bias_A2_UNorm:
                case Format.B8G8R8A8_Typeless:
                case Format.B8G8R8A8_UNorm_SRgb:
                case Format.B8G8R8X8_Typeless:
                case Format.B8G8R8X8_UNorm_SRgb:
                case Format.BC6H_Typeless:
                case Format.BC6H_Uf16:
                case Format.BC6H_Sf16:
                case Format.BC7_Typeless:
                case Format.BC7_UNorm:
                case Format.BC7_UNorm_SRgb:
                case Format.AYUV:
                case Format.Y410:
                case Format.Y416:
                case Format.NV12:
                case Format.P010:
                case Format.P016:
                case Format.Opaque420:
                case Format.YUY2:
                case Format.Y210:
                case Format.Y216:
                case Format.NV11:
                case Format.AI44:
                case Format.IA44:
                case Format.P8:
                case Format.A8P8:
                case Format.B4G4R4A4_UNorm:
                case Format.P208:
                case Format.V208:
                case Format.V408:
                    throw new NotImplementedException();
                case Format.Unknown:
                default:
                    throw Illegal.Value<Format>();
            }
        }
    }
}