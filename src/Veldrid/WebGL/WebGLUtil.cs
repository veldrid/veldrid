using System;
using static Veldrid.WebGL.WebGLConstants;

namespace Veldrid.WebGL
{
    internal static class WebGLUtil
    {
        public static uint GetShaderStage(ShaderStages stage)
        {
            switch (stage)
            {
                case ShaderStages.Vertex:
                    return VERTEX_SHADER;
                case ShaderStages.Fragment:
                    return FRAGMENT_SHADER;
                default:
                    throw Illegal.Value<ShaderStages>();
            }
        }

        internal static uint VdToGLDrawElementsType(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt16:
                    return UNSIGNED_SHORT;
                case IndexFormat.UInt32:
                    return UNSIGNED_INT;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        internal static uint VdToGLBlendFactor(BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Zero:
                    return ZERO;
                case BlendFactor.One:
                    return ONE;
                case BlendFactor.SourceAlpha:
                    return SRC_ALPHA;
                case BlendFactor.InverseSourceAlpha:
                    return ONE_MINUS_SRC_ALPHA;
                case BlendFactor.DestinationAlpha:
                    return DST_ALPHA;
                case BlendFactor.InverseDestinationAlpha:
                    return ONE_MINUS_DST_ALPHA;
                case BlendFactor.SourceColor:
                    return SRC_COLOR;
                case BlendFactor.InverseSourceColor:
                    return ONE_MINUS_SRC_COLOR;
                case BlendFactor.DestinationColor:
                    return DST_COLOR;
                case BlendFactor.InverseDestinationColor:
                    return ONE_MINUS_DST_COLOR;
                case BlendFactor.BlendFactor:
                    return CONSTANT_COLOR;
                case BlendFactor.InverseBlendFactor:
                    return ONE_MINUS_CONSTANT_COLOR;
                default:
                    throw Illegal.Value<BlendFactor>();
            }
        }

        internal static uint VdToGLPixelType(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                case PixelFormat.R8_UInt:
                case PixelFormat.R8_G8_UNorm:
                case PixelFormat.R8_G8_UInt:
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                case PixelFormat.R8_G8_B8_A8_UInt:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return UNSIGNED_BYTE;
                case PixelFormat.R8_SNorm:
                case PixelFormat.R8_SInt:
                case PixelFormat.R8_G8_SNorm:
                case PixelFormat.R8_G8_SInt:
                case PixelFormat.R8_G8_B8_A8_SNorm:
                case PixelFormat.R8_G8_B8_A8_SInt:
                case PixelFormat.BC4_SNorm:
                case PixelFormat.BC5_SNorm:
                    return BYTE;
                case PixelFormat.R16_UNorm:
                case PixelFormat.R16_UInt:
                case PixelFormat.R16_G16_UNorm:
                case PixelFormat.R16_G16_UInt:
                case PixelFormat.R16_G16_B16_A16_UNorm:
                case PixelFormat.R16_G16_B16_A16_UInt:
                    return UNSIGNED_SHORT;
                case PixelFormat.R16_SNorm:
                case PixelFormat.R16_SInt:
                case PixelFormat.R16_G16_SNorm:
                case PixelFormat.R16_G16_SInt:
                case PixelFormat.R16_G16_B16_A16_SNorm:
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return SHORT;
                case PixelFormat.R32_UInt:
                case PixelFormat.R32_G32_UInt:
                case PixelFormat.R32_G32_B32_A32_UInt:
                    return UNSIGNED_INT;
                case PixelFormat.R32_SInt:
                case PixelFormat.R32_G32_SInt:
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return INT;
                case PixelFormat.R16_Float:
                case PixelFormat.R16_G16_Float:
                case PixelFormat.R16_G16_B16_A16_Float:
                    return HALF_FLOAT;
                case PixelFormat.R32_Float:
                case PixelFormat.R32_G32_Float:
                case PixelFormat.R32_G32_B32_A32_Float:
                    return FLOAT;

                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return UNSIGNED_BYTE; // ?

                case PixelFormat.D32_Float_S8_UInt:
                    return FLOAT_32_UNSIGNED_INT_24_8_REV;
                case PixelFormat.D24_UNorm_S8_UInt:
                    return UNSIGNED_INT_24_8;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return UNSIGNED_INT_2_10_10_10_REV;
                case PixelFormat.R11_G11_B10_Float:
                    return UNSIGNED_INT_10F_11F_11F_REV;

                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static uint VdToGLPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                case PixelFormat.R16_UNorm:
                case PixelFormat.R16_Float:
                case PixelFormat.R32_Float:
                case PixelFormat.BC4_UNorm:
                    return RED;

                case PixelFormat.R8_SNorm:
                case PixelFormat.R8_UInt:
                case PixelFormat.R8_SInt:
                case PixelFormat.R16_SNorm:
                case PixelFormat.R16_UInt:
                case PixelFormat.R16_SInt:
                case PixelFormat.R32_UInt:
                case PixelFormat.R32_SInt:
                case PixelFormat.BC4_SNorm:
                    return RED_INTEGER;

                case PixelFormat.R8_G8_UNorm:
                case PixelFormat.R16_G16_UNorm:
                case PixelFormat.R16_G16_Float:
                case PixelFormat.R32_G32_Float:
                case PixelFormat.BC5_UNorm:
                    return RG;

                case PixelFormat.R8_G8_SNorm:
                case PixelFormat.R8_G8_UInt:
                case PixelFormat.R8_G8_SInt:
                case PixelFormat.R16_G16_SNorm:
                case PixelFormat.R16_G16_UInt:
                case PixelFormat.R16_G16_SInt:
                case PixelFormat.R32_G32_UInt:
                case PixelFormat.R32_G32_SInt:
                case PixelFormat.BC5_SNorm:
                    return RG_INTEGER;

                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                case PixelFormat.R16_G16_B16_A16_UNorm:
                case PixelFormat.R16_G16_B16_A16_Float:
                case PixelFormat.R32_G32_B32_A32_Float:
                    return RGBA;

                case PixelFormat.B8_G8_R8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return RGBA;

                case PixelFormat.R8_G8_B8_A8_SNorm:
                case PixelFormat.R8_G8_B8_A8_UInt:
                case PixelFormat.R8_G8_B8_A8_SInt:
                case PixelFormat.R16_G16_B16_A16_SNorm:
                case PixelFormat.R16_G16_B16_A16_UInt:
                case PixelFormat.R16_G16_B16_A16_SInt:
                case PixelFormat.R32_G32_B32_A32_UInt:
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return RGBA_INTEGER;

                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                    return RGB;
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    return RGBA;

                case PixelFormat.D24_UNorm_S8_UInt:
                case PixelFormat.D32_Float_S8_UInt:
                    return DEPTH_STENCIL;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return RGBA;
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return RGBA_INTEGER;
                case PixelFormat.R11_G11_B10_Float:
                    return RGB;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static uint VdToGLPixelInternalFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                    return R8;
                case PixelFormat.R8_SNorm:
                    return R8_SNORM;
                case PixelFormat.R8_UInt:
                    return R8UI;
                case PixelFormat.R8_SInt:
                    return R8I;

                case PixelFormat.R16_UNorm:
                    return DEPTH_COMPONENT16;
                case PixelFormat.R16_UInt:
                    return R16UI;
                case PixelFormat.R16_SInt:
                    return R16I;
                case PixelFormat.R16_Float:
                    return R16F;
                case PixelFormat.R32_UInt:
                    return R32UI;
                case PixelFormat.R32_SInt:
                    return R32I;
                case PixelFormat.R32_Float:
                    return R32F;

                case PixelFormat.R8_G8_UNorm:
                    return RG8;
                case PixelFormat.R8_G8_SNorm:
                    return RG8_SNORM;
                case PixelFormat.R8_G8_UInt:
                    return RG8UI;
                case PixelFormat.R8_G8_SInt:
                    return RG8I;

                case PixelFormat.R16_G16_UInt:
                    return RG16UI;
                case PixelFormat.R16_G16_SInt:
                    return RG16I;
                case PixelFormat.R16_G16_Float:
                    return RG16F;

                case PixelFormat.R32_G32_UInt:
                    return RG32UI;
                case PixelFormat.R32_G32_SInt:
                    return RG32I;
                case PixelFormat.R32_G32_Float:
                    return RG32F;

                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return RGBA8;
                case PixelFormat.R8_G8_B8_A8_UNorm_SRgb:
                    return SRGB8_ALPHA8;
                case PixelFormat.R8_G8_B8_A8_SNorm:
                    return RGBA8_SNORM;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return RGBA8UI;
                case PixelFormat.R8_G8_B8_A8_SInt:
                    return RGBA8I;

                case PixelFormat.R16_G16_B16_A16_UInt:
                    return RGBA16UI;
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return RGBA16I;
                case PixelFormat.R16_G16_B16_A16_Float:
                    return RGBA16F;

                case PixelFormat.R32_G32_B32_A32_Float:
                    return RGBA32F;
                case PixelFormat.R32_G32_B32_A32_UInt:
                    return RGBA32UI;
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return RGBA32I;

                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return RGBA;
                case PixelFormat.B8_G8_R8_A8_UNorm_SRgb:
                    return SRGB8_ALPHA8;
                    
                case PixelFormat.BC1_Rgb_UNorm:
                    return COMPRESSED_RGB_S3TC_DXT1_EXT;
                case PixelFormat.BC1_Rgba_UNorm:
                    return COMPRESSED_RGBA_S3TC_DXT1_EXT;
                case PixelFormat.BC2_UNorm:
                    return COMPRESSED_RGBA_S3TC_DXT3_EXT;
                case PixelFormat.BC3_UNorm:
                    return COMPRESSED_RGBA_S3TC_DXT5_EXT;
                case PixelFormat.BC4_UNorm:
                    return COMPRESSED_RED_RGTC1_EXT;
                case PixelFormat.BC4_SNorm:
                    return COMPRESSED_SIGNED_RED_RGTC1_EXT;
                case PixelFormat.BC5_UNorm:
                    return COMPRESSED_RED_GREEN_RGTC2_EXT;
                case PixelFormat.BC5_SNorm:
                    return COMPRESSED_SIGNED_RED_GREEN_RGTC2_EXT;
                case PixelFormat.BC7_UNorm:
                    return COMPRESSED_RGBA_BPTC_UNORM_EXT;
                case PixelFormat.BC7_UNorm_SRgb:
                    return COMPRESSED_SRGB_ALPHA_BPTC_UNORM_EXT;

                case PixelFormat.D32_Float_S8_UInt:
                    return DEPTH32F_STENCIL8;
                case PixelFormat.D24_UNorm_S8_UInt:
                    return DEPTH24_STENCIL8;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return RGB10_A2;
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return RGB10_A2UI;
                case PixelFormat.R11_G11_B10_Float:
                    return R11F_G11F_B10F;

                default:
                    Console.WriteLine($"Illegal pixel format: {format}");
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static uint VdToGLBlendEquationMode(BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return FUNC_ADD;
                case BlendFunction.Subtract:
                    return FUNC_SUBTRACT;
                case BlendFunction.ReverseSubtract:
                    return FUNC_REVERSE_SUBTRACT;
                case BlendFunction.Minimum:
                    return MIN;
                case BlendFunction.Maximum:
                    return MAX;
                default:
                    throw Illegal.Value<BlendFunction>();
            }
        }

        internal static uint ConvertComparisonKind(ComparisonKind value)
        {
            switch (value)
            {
                case ComparisonKind.Never:
                    return NEVER;
                case ComparisonKind.Less:
                    return LESS;
                case ComparisonKind.Equal:
                    return EQUAL;
                case ComparisonKind.LessEqual:
                    return LEQUAL;
                case ComparisonKind.Greater:
                    return GREATER;
                case ComparisonKind.NotEqual:
                    return NOTEQUAL;
                case ComparisonKind.GreaterEqual:
                    return GEQUAL;
                case ComparisonKind.Always:
                    return ALWAYS;
                default:
                    throw Illegal.Value<ComparisonKind>();
            }
        }

        internal static uint VdToGLCullFaceMode(FaceCullMode cullMode)
        {
            switch (cullMode)
            {
                case FaceCullMode.Back:
                    return BACK;
                case FaceCullMode.Front:
                    return FRONT;
                default:
                    throw Illegal.Value<FaceCullMode>();
            }
        }

        internal static uint VdToGLFrontFaceDirection(FrontFace frontFace)
        {
            switch (frontFace)
            {
                case FrontFace.Clockwise:
                    return CW;
                case FrontFace.CounterClockwise:
                    return CCW;
                default:
                    throw Illegal.Value<FrontFace>();
            }
        }

        internal static uint VdToGLPrimitiveType(PrimitiveTopology primitiveTopology)
        {
            switch (primitiveTopology)
            {
                case PrimitiveTopology.TriangleList:
                    return TRIANGLES;
                case PrimitiveTopology.TriangleStrip:
                    return TRIANGLE_STRIP;
                case PrimitiveTopology.LineList:
                    return LINES;
                case PrimitiveTopology.LineStrip:
                    return LINE_STRIP;
                case PrimitiveTopology.PointList:
                    return POINTS;
                default:
                    throw Illegal.Value<PrimitiveTopology>();
            }
        }

        internal static uint VdToGLVertexAttribPointerType(
            VertexElementFormat format,
            out bool normalized,
            out bool isInteger)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                case VertexElementFormat.Float2:
                case VertexElementFormat.Float3:
                case VertexElementFormat.Float4:
                    normalized = false;
                    isInteger = false;
                    return FLOAT;
                case VertexElementFormat.Half1:
                case VertexElementFormat.Half2:
                case VertexElementFormat.Half4:
                    normalized = false;
                    isInteger = false;
                    return HALF_FLOAT;
                case VertexElementFormat.Byte2_Norm:
                case VertexElementFormat.Byte4_Norm:
                    normalized = true;
                    isInteger = true;
                    return UNSIGNED_BYTE;
                case VertexElementFormat.Byte2:
                case VertexElementFormat.Byte4:
                    normalized = false;
                    isInteger = true;
                    return UNSIGNED_BYTE;
                case VertexElementFormat.SByte2_Norm:
                case VertexElementFormat.SByte4_Norm:
                    normalized = true;
                    isInteger = true;
                    return BYTE;
                case VertexElementFormat.SByte2:
                case VertexElementFormat.SByte4:
                    normalized = false;
                    isInteger = true;
                    return BYTE;
                case VertexElementFormat.UShort2_Norm:
                case VertexElementFormat.UShort4_Norm:
                    normalized = true;
                    isInteger = true;
                    return UNSIGNED_SHORT;
                case VertexElementFormat.UShort2:
                case VertexElementFormat.UShort4:
                    normalized = false;
                    isInteger = true;
                    return UNSIGNED_SHORT;
                case VertexElementFormat.Short2_Norm:
                case VertexElementFormat.Short4_Norm:
                    normalized = true;
                    isInteger = true;
                    return SHORT;
                case VertexElementFormat.Short2:
                case VertexElementFormat.Short4:
                    normalized = false;
                    isInteger = true;
                    return SHORT;
                case VertexElementFormat.UInt1:
                case VertexElementFormat.UInt2:
                case VertexElementFormat.UInt3:
                case VertexElementFormat.UInt4:
                    normalized = false;
                    isInteger = true;
                    return UNSIGNED_INT;
                case VertexElementFormat.Int1:
                case VertexElementFormat.Int2:
                case VertexElementFormat.Int3:
                case VertexElementFormat.Int4:
                    normalized = false;
                    isInteger = true;
                    return INT;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }
    }
}
