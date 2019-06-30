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
