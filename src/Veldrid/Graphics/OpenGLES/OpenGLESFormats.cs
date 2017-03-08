using System;
using OpenTK.Graphics.ES30;

namespace Veldrid.Graphics.OpenGLES
{
    public static class OpenGLESFormats
    {
        public static OpenTK.Graphics.ES30.PixelFormat MapPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return OpenTK.Graphics.ES30.PixelFormat.Rgba;
                case PixelFormat.R8_UInt:
                    return OpenTK.Graphics.ES30.PixelFormat.RedInteger;
                case PixelFormat.Alpha_UInt8:
                    return OpenTK.Graphics.ES30.PixelFormat.Alpha;
                case PixelFormat.R8_G8_B8_A8:
                    return OpenTK.Graphics.ES30.PixelFormat.Rgba;
                case PixelFormat.Alpha_UInt16:
                    return OpenTK.Graphics.ES30.PixelFormat.Alpha;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        public static TextureComponentCount MapTextureComponentCount(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return TextureComponentCount.Rgba;
                case PixelFormat.R8_UInt:
                    return TextureComponentCount.R8ui;
                case PixelFormat.Alpha_UInt8:
                    return TextureComponentCount.R8ui;
                case PixelFormat.R8_G8_B8_A8:
                    return TextureComponentCount.Rgba8;
                case PixelFormat.Alpha_UInt16:
                    return TextureComponentCount.DepthComponent16; // HACK
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        public static OpenTK.Graphics.ES30.ShaderType VeldridToGLShaderType(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.Vertex:
                    return OpenTK.Graphics.ES30.ShaderType.VertexShader;
                case ShaderType.Fragment:
                    return OpenTK.Graphics.ES30.ShaderType.FragmentShader;
                case ShaderType.Geometry:
                    throw new NotSupportedException($"Shaders of type {type} are not supported in OpenGL ES.");
                default:
                    throw Illegal.Value<ShaderType>();
            }
        }

        public static ShaderType GLToVeldridShaderType(OpenTK.Graphics.ES30.ShaderType type)
        {
            switch (type)
            {
                case OpenTK.Graphics.ES30.ShaderType.FragmentShader:
                    return ShaderType.Fragment;
                case OpenTK.Graphics.ES30.ShaderType.VertexShader:
                    return ShaderType.Vertex;
                    throw new NotSupportedException($"The shader type {type} is not supported.");
                default:
                    throw Illegal.Value<OpenTK.Graphics.ES30.ShaderType>();
            }
        }

        internal static DepthFunction ConvertDepthComparison(DepthComparison depthComparison)
        {
            switch (depthComparison)
            {
                case DepthComparison.Never:
                    return DepthFunction.Never;
                case DepthComparison.Less:
                    return DepthFunction.Less;
                case DepthComparison.Equal:
                    return DepthFunction.Equal;
                case DepthComparison.LessEqual:
                    return DepthFunction.Lequal;
                case DepthComparison.Greater:
                    return DepthFunction.Greater;
                case DepthComparison.NotEqual:
                    return DepthFunction.Notequal;
                case DepthComparison.GreaterEqual:
                    return DepthFunction.Gequal;
                case DepthComparison.Always:
                    return DepthFunction.Always;
                default:
                    throw Illegal.Value<DepthComparison>();
            }
        }

        internal static PolygonMode ConvertFillMode(TriangleFillMode fillMode)
        {
            switch (fillMode)
            {
                case TriangleFillMode.Solid:
                    return PolygonMode.Fill;
                case TriangleFillMode.Wireframe:
                    return PolygonMode.Line;
                default:
                    throw Illegal.Value<TriangleFillMode>();
            }
        }

        internal static CullFaceMode ConvertCullMode(FaceCullingMode cullMode)
        {
            switch (cullMode)
            {
                case FaceCullingMode.Back:
                    return CullFaceMode.Back;
                case FaceCullingMode.Front:
                    return CullFaceMode.Front;
                default:
                    // FaceCullingMode.None should not be converted.
                    // It translates to GL.Disable(EnableCap.CullFace).
                    throw Illegal.Value<FaceCullingMode>();
            }
        }

        public static PixelType MapPixelType(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return PixelType.Float;
                case PixelFormat.R8_UInt:
                    return PixelType.UnsignedByte;
                case PixelFormat.Alpha_UInt8:
                    return PixelType.UnsignedByte;
                case PixelFormat.R8_G8_B8_A8:
                    return PixelType.UnsignedByte;
                case PixelFormat.Alpha_UInt16:
                    return PixelType.UnsignedShort;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static PixelInternalFormat MapPixelInternalFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return PixelInternalFormat.Rgba;
                case PixelFormat.R8_UInt:
                    return PixelInternalFormat.Alpha;
                case PixelFormat.Alpha_UInt8:
                    return PixelInternalFormat.Alpha;
                case PixelFormat.R8_G8_B8_A8:
                    return PixelInternalFormat.Rgba;
                case PixelFormat.Alpha_UInt16:
                    return PixelInternalFormat.Alpha;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static BlendingFactorSrc ConvertBlendSrc(Blend blend)
        {
            switch (blend)
            {
                case Blend.Zero:
                    return BlendingFactorSrc.Zero;
                case Blend.One:
                    return BlendingFactorSrc.One;
                case Blend.SourceAlpha:
                    return BlendingFactorSrc.SrcAlpha;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorSrc.OneMinusSrcAlpha;
                case Blend.DestinationAlpha:
                    return BlendingFactorSrc.DstAlpha;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorSrc.OneMinusDstAlpha;
                case Blend.SourceColor:
                    return BlendingFactorSrc.SrcColor;
                case Blend.InverseSourceColor:
                    return BlendingFactorSrc.OneMinusSrcColor;
                case Blend.DestinationColor:
                    return BlendingFactorSrc.DstColor;
                case Blend.InverseDestinationColor:
                    return BlendingFactorSrc.OneMinusDstColor;
                case Blend.BlendFactor:
                    return BlendingFactorSrc.ConstantColor;
                case Blend.InverseBlendFactor:
                    return BlendingFactorSrc.OneMinusConstantColor;
                default:
                    throw Illegal.Value<Blend>();
            }
        }

        internal static PrimitiveType ConvertPrimitiveTopology(PrimitiveTopology primitiveTopology)
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

        internal static BlendingFactorDest ConvertBlendDest(Blend blend) => (BlendingFactorDest)ConvertBlendSrc(blend);

        public static BlendEquationMode ConvertBlendEquation(BlendFunction function)
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
                    throw new PlatformNotSupportedException();
                case BlendFunction.Maximum:
                    throw new PlatformNotSupportedException();
                default:
                    throw Illegal.Value<BlendFunction>();
            }
        }

        internal static DrawElementsType MapIndexFormat(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt32:
                    throw new PlatformNotSupportedException();
                case IndexFormat.UInt16:
                    return DrawElementsType.UnsignedShort;
                case IndexFormat.UInt8:
                    return DrawElementsType.UnsignedByte;
                default:
                    throw Illegal.Value<DrawElementsType>();
            }
        }

        public static int GetIndexFormatSize(DrawElementsType type)
        {
            switch (type)
            {
                case DrawElementsType.UnsignedByte:
                    return 1;
                case DrawElementsType.UnsignedShort:
                    return 2;
                default:
                    throw Illegal.Value<DrawElementsType>();
            }
        }
    }
}