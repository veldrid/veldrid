using System;
using System.Diagnostics;
using Veldrid.OpenGLBinding;

namespace Veldrid.OpenGL
{
    internal static class OpenGLFormats
    {
        internal static DrawElementsType VdToGLDrawElementsType(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt16:
                    return DrawElementsType.UnsignedShort;
                case IndexFormat.UInt32:
                    return DrawElementsType.UnsignedInt;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        internal static ShaderType VdToGLShaderType(ShaderStages stage)
        {
            switch (stage)
            {
                case ShaderStages.Vertex:
                    return ShaderType.VertexShader;
                case ShaderStages.Geometry:
                    return ShaderType.GeometryShader;
                case ShaderStages.TessellationControl:
                    return ShaderType.TessControlShader;
                case ShaderStages.TessellationEvaluation:
                    return ShaderType.TessEvaluationShader;
                case ShaderStages.Fragment:
                    return ShaderType.FragmentShader;
                case ShaderStages.Compute:
                    return ShaderType.ComputeShader;
                default:
                    throw Illegal.Value<ShaderStages>();
            }
        }

        internal static PixelInternalFormat VdToGLPixelInternalFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return PixelInternalFormat.Rgba;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return PixelInternalFormat.Rgba;
                case PixelFormat.R8_UNorm:
                    return PixelInternalFormat.R8ui;
                case PixelFormat.R16_UNorm:
                    return PixelInternalFormat.R16ui;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return PixelInternalFormat.Rgba32f;
                case PixelFormat.R32_Float:
                    return PixelInternalFormat.R32f;
                case PixelFormat.BC3_UNorm:
                    return PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                case PixelFormat.D32_Float_S8_UInt:
                    return PixelInternalFormat.Depth32fStencil8;
                case PixelFormat.D24_UNorm_S8_UInt:
                    return PixelInternalFormat.Depth24Stencil8;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static TextureWrapMode VdToGLTextureWrapMode(SamplerAddressMode mode)
        {
            switch (mode)
            {
                case SamplerAddressMode.Wrap:
                    return TextureWrapMode.Repeat;
                case SamplerAddressMode.Mirror:
                    return TextureWrapMode.MirroredRepeat;
                case SamplerAddressMode.Clamp:
                    return TextureWrapMode.Clamp;
                case SamplerAddressMode.Border:
                    return TextureWrapMode.ClampToBorder;
                default:
                    throw Illegal.Value<SamplerAddressMode>();
            }
        }

        internal static GLPixelFormat VdToGLPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return GLPixelFormat.Rgba;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return GLPixelFormat.Bgra;
                case PixelFormat.R8_UNorm:
                    return GLPixelFormat.Red;
                case PixelFormat.R16_UNorm:
                    return GLPixelFormat.Red;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return GLPixelFormat.Rgba;
                case PixelFormat.R32_Float:
                    return GLPixelFormat.Red;
                case PixelFormat.BC3_UNorm:
                    return GLPixelFormat.Rgba;
                case PixelFormat.D24_UNorm_S8_UInt:
                    return GLPixelFormat.DepthStencil;
                case PixelFormat.D32_Float_S8_UInt:
                    return GLPixelFormat.DepthStencil;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static GLPixelType VdToGLPixelType(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.R8_UNorm:
                    return GLPixelType.UnsignedByte;
                case PixelFormat.R16_UNorm:
                    return GLPixelType.UnsignedShort;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return GLPixelType.Float;
                case PixelFormat.R32_Float:
                    return GLPixelType.Float;
                case PixelFormat.BC3_UNorm:
                    return GLPixelType.UnsignedByte; // ?
                case PixelFormat.D32_Float_S8_UInt:
                    return GLPixelType.Float32UnsignedInt248Rev;
                case PixelFormat.D24_UNorm_S8_UInt:
                    return GLPixelType.UnsignedInt248;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static SizedInternalFormat VdToGLSizedInternalFormat(PixelFormat format, bool depthFormat)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return SizedInternalFormat.Rgba8;
                case PixelFormat.R8_UNorm:
                    return SizedInternalFormat.R8;
                case PixelFormat.R16_UNorm:
                    return depthFormat ? (SizedInternalFormat)PixelInternalFormat.DepthComponent16 : SizedInternalFormat.R16;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return SizedInternalFormat.Rgba32f;
                case PixelFormat.R32_Float:
                    return depthFormat ? (SizedInternalFormat)PixelInternalFormat.DepthComponent32f : SizedInternalFormat.R32f;
                case PixelFormat.BC3_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                case PixelFormat.D32_Float_S8_UInt:
                    Debug.Assert(depthFormat);
                    return (SizedInternalFormat)PixelInternalFormat.Depth32fStencil8;
                case PixelFormat.D24_UNorm_S8_UInt:
                    Debug.Assert(depthFormat);
                    return (SizedInternalFormat)PixelInternalFormat.Depth24Stencil8;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static void VdToGLTextureMinMagFilter(SamplerFilter filter, bool mip, out TextureMinFilter min, out TextureMagFilter mag)
        {
            switch (filter)
            {
                case SamplerFilter.Anisotropic:
                case SamplerFilter.MinPoint_MagPoint_MipPoint:
                    min = mip ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest;
                    mag = TextureMagFilter.Nearest;
                    break;
                case SamplerFilter.MinPoint_MagPoint_MipLinear:
                    min = mip ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest;
                    mag = TextureMagFilter.Nearest;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipPoint:
                    min = mip ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest;
                    mag = TextureMagFilter.Linear;
                    break;
                case SamplerFilter.MinPoint_MagLinear_MipLinear:
                    min = mip ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest;
                    mag = TextureMagFilter.Linear;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipPoint:
                    min = mip ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear;
                    mag = TextureMagFilter.Nearest;
                    break;
                case SamplerFilter.MinLinear_MagPoint_MipLinear:
                    min = mip ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
                    mag = TextureMagFilter.Nearest;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipPoint:
                    min = mip ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear;
                    mag = TextureMagFilter.Linear;
                    break;
                case SamplerFilter.MinLinear_MagLinear_MipLinear:
                    min = mip ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
                    mag = TextureMagFilter.Linear;
                    break;
                default:
                    throw Illegal.Value<SamplerFilter>();
            }
        }

        internal static BufferAccessMask VdToGLMapMode(MapMode mode)
        {
            switch (mode)
            {
                case MapMode.Read:
                    return BufferAccessMask.Read;
                case MapMode.Write:
                    return BufferAccessMask.Write | BufferAccessMask.InvalidateBuffer;
                case MapMode.ReadWrite:
                    return BufferAccessMask.Read | BufferAccessMask.Write;
                default:
                    throw Illegal.Value<MapMode>();
            }
        }

        internal static VertexAttribPointerType VdToGLVertexAttribPointerType(VertexElementFormat format, out bool normalized)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                case VertexElementFormat.Float2:
                case VertexElementFormat.Float3:
                case VertexElementFormat.Float4:
                    normalized = false; // N/A
                    return VertexAttribPointerType.Float;
                case VertexElementFormat.Byte2_UNorm:
                case VertexElementFormat.Byte4_UNorm:
                    normalized = true;
                    return VertexAttribPointerType.UnsignedByte;
                case VertexElementFormat.Byte2_UInt:
                case VertexElementFormat.Byte4_UInt:
                    normalized = false;
                    return VertexAttribPointerType.UnsignedByte;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        internal static DepthFunction VdToGLDepthFunction(ComparisonKind value)
        {
            switch (value)
            {
                case ComparisonKind.Never:
                    return DepthFunction.Never;
                case ComparisonKind.Less:
                    return DepthFunction.Less;
                case ComparisonKind.Equal:
                    return DepthFunction.Equal;
                case ComparisonKind.LessEqual:
                    return DepthFunction.Lequal;
                case ComparisonKind.Greater:
                    return DepthFunction.Greater;
                case ComparisonKind.NotEqual:
                    return DepthFunction.Notequal;
                case ComparisonKind.GreaterEqual:
                    return DepthFunction.Gequal;
                case ComparisonKind.Always:
                    return DepthFunction.Always;
                default:
                    throw Illegal.Value<ComparisonKind>();
            }
        }

        internal static BlendingFactorSrc VdToGLBlendFactorSrc(BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Zero:
                    return BlendingFactorSrc.Zero;
                case BlendFactor.One:
                    return BlendingFactorSrc.One;
                case BlendFactor.SourceAlpha:
                    return BlendingFactorSrc.SrcAlpha;
                case BlendFactor.InverseSourceAlpha:
                    return BlendingFactorSrc.OneMinusSrcAlpha;
                case BlendFactor.DestinationAlpha:
                    return BlendingFactorSrc.DstAlpha;
                case BlendFactor.InverseDestinationAlpha:
                    return BlendingFactorSrc.OneMinusDstAlpha;
                case BlendFactor.SourceColor:
                    return BlendingFactorSrc.SrcColor;
                case BlendFactor.InverseSourceColor:
                    return BlendingFactorSrc.OneMinusSrcColor;
                case BlendFactor.DestinationColor:
                    return BlendingFactorSrc.DstColor;
                case BlendFactor.InverseDestinationColor:
                    return BlendingFactorSrc.OneMinusDstColor;
                case BlendFactor.BlendFactor:
                    return BlendingFactorSrc.ConstantColor;
                case BlendFactor.InverseBlendFactor:
                    return BlendingFactorSrc.OneMinusConstantColor;
                default:
                    throw Illegal.Value<BlendFactor>();
            }
        }

        internal static BlendEquationMode VdToGLBlendEquationMode(BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return BlendEquationMode.FuncAdd;
                case BlendFunction.Subtract:
                    return BlendEquationMode.FuncSubtract;
                case BlendFunction.ReverseSubtract:
                    return BlendEquationMode.FuncReverseSubtract;
                case BlendFunction.Minimum:
                    return BlendEquationMode.Min;
                case BlendFunction.Maximum:
                    return BlendEquationMode.Max;
                default:
                    throw Illegal.Value<BlendFunction>();
            }
        }

        internal static PolygonMode VdToGLPolygonMode(PolygonFillMode fillMode)
        {
            switch (fillMode)
            {
                case PolygonFillMode.Solid:
                    return PolygonMode.Fill;
                case PolygonFillMode.Wireframe:
                    return PolygonMode.Line;
                default:
                    throw Illegal.Value<PolygonFillMode>();
            }
        }

        internal static StencilFunction VdToGLStencilFunction(ComparisonKind comparison)
        {
            switch (comparison)
            {
                case ComparisonKind.Never:
                    return StencilFunction.Never;
                case ComparisonKind.Less:
                    return StencilFunction.Less;
                case ComparisonKind.Equal:
                    return StencilFunction.Equal;
                case ComparisonKind.LessEqual:
                    return StencilFunction.Lequal;
                case ComparisonKind.Greater:
                    return StencilFunction.Greater;
                case ComparisonKind.NotEqual:
                    return StencilFunction.Notequal;
                case ComparisonKind.GreaterEqual:
                    return StencilFunction.Gequal;
                case ComparisonKind.Always:
                    return StencilFunction.Always;
                default:
                    throw Illegal.Value<ComparisonKind>();
            }
        }

        internal static CullFaceMode VdToGLCullFaceMode(FaceCullMode cullMode)
        {
            switch (cullMode)
            {
                case FaceCullMode.Back:
                    return CullFaceMode.Back;
                case FaceCullMode.Front:
                    return CullFaceMode.Front;
                default:
                    throw Illegal.Value<FaceCullMode>();
            }
        }

        internal static PrimitiveType VdToGLPrimitiveType(PrimitiveTopology primitiveTopology)
        {
            switch (primitiveTopology)
            {
                case PrimitiveTopology.TriangleList:
                    return PrimitiveType.Triangles;
                case PrimitiveTopology.TriangleStrip:
                    return PrimitiveType.TriangleStrip;
                case PrimitiveTopology.LineList:
                    return PrimitiveType.Lines;
                case PrimitiveTopology.LineStrip:
                    return PrimitiveType.LineStrip;
                case PrimitiveTopology.PointList:
                    return PrimitiveType.Points;
                default:
                    throw Illegal.Value<PrimitiveTopology>();
            }
        }

        internal static FrontFaceDirection VdToGLFrontFaceDirection(FrontFace frontFace)
        {
            switch (frontFace)
            {
                case FrontFace.Clockwise:
                    return FrontFaceDirection.Cw;
                case FrontFace.CounterClockwise:
                    return FrontFaceDirection.Ccw;
                default:
                    throw Illegal.Value<FrontFace>();
            }
        }

        internal static BlendingFactorDest VdToGLBlendFactorDest(BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Zero:
                    return BlendingFactorDest.Zero;
                case BlendFactor.One:
                    return BlendingFactorDest.One;
                case BlendFactor.SourceAlpha:
                    return BlendingFactorDest.SrcAlpha;
                case BlendFactor.InverseSourceAlpha:
                    return BlendingFactorDest.OneMinusSrcAlpha;
                case BlendFactor.DestinationAlpha:
                    return BlendingFactorDest.DstAlpha;
                case BlendFactor.InverseDestinationAlpha:
                    return BlendingFactorDest.OneMinusDstAlpha;
                case BlendFactor.SourceColor:
                    return BlendingFactorDest.SrcColor;
                case BlendFactor.InverseSourceColor:
                    return BlendingFactorDest.OneMinusSrcColor;
                case BlendFactor.DestinationColor:
                    return BlendingFactorDest.DstColor;
                case BlendFactor.InverseDestinationColor:
                    return BlendingFactorDest.OneMinusDstColor;
                case BlendFactor.BlendFactor:
                    return BlendingFactorDest.ConstantColor;
                case BlendFactor.InverseBlendFactor:
                    return BlendingFactorDest.OneMinusConstantColor;
                default:
                    throw Illegal.Value<BlendFactor>();
            }
        }
    }
}