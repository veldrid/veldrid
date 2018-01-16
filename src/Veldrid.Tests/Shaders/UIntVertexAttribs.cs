using ShaderGen;
using System.Numerics;

[assembly: ShaderSet("UIntVertexAttribs", "Veldrid.Tests.Shaders.UIntVertexAttribs.VS", "Veldrid.Tests.Shaders.UIntVertexAttribs.FS")]

namespace Veldrid.Tests.Shaders
{
    internal class UIntVertexAttribs
    {
        public struct Vertex
        {
            [PositionSemantic]
            public Vector2 Position;
            [ColorSemantic]
            public UInt4 Color_Int;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic]
            public Vector4 Position;
            [ColorSemantic]
            public Vector4 Color;
        }

        public struct Info
        {
            public uint ColorNormalizationFactor;
            private float padding0;
            private float padding1;
            private float padding2;
        }

        public Info InfoBuffer;
        public Matrix4x4 Ortho;

        [VertexShader]
        public FragmentInput VS(Vertex input)
        {
            FragmentInput output;
            output.Position = ShaderBuiltins.Mul(Ortho, new Vector4(input.Position, 0, 1));
            output.Color = new Vector4(input.Color_Int.X, input.Color_Int.Y, input.Color_Int.Z, 1) / InfoBuffer.ColorNormalizationFactor;
            output.Color.W = 1;
            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            //return new Vector4(0.25f, 0.66f, 0.125f, 1f);
            return input.Color;
        }
    }
}
