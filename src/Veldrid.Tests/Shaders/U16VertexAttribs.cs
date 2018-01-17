using ShaderGen;
using System.Numerics;
using System.Runtime.InteropServices;

[assembly: ShaderSet("U16VertexAttribs", "Veldrid.Tests.Shaders.U16VertexAttribs.VS", "Veldrid.Tests.Shaders.U16VertexAttribs.FS")]

namespace Veldrid.Tests.Shaders
{
    public class U16VertexAttribs
    {
        public struct VertexGPU
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

        [StructLayout(LayoutKind.Sequential)]
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
        public FragmentInput VS(VertexGPU input)
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
            return input.Color;
        }
    }
}
