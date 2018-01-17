using ShaderGen;
using System.Numerics;

[assembly: ShaderSet("U16NormVertexAttribs", "Veldrid.Tests.Shaders.U16NormVertexAttribs.VS", "Veldrid.Tests.Shaders.U16NormVertexAttribs.FS")]

namespace Veldrid.Tests.Shaders
{
    public class U16NormVertexAttribs
    {
        public struct VertexGPU
        {
            [PositionSemantic]
            public Vector2 Position;
            [ColorSemantic]
            public Vector4 Color;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic]
            public Vector4 Position;
            [ColorSemantic]
            public Vector4 Color;
        }

        public Matrix4x4 Ortho;

        [VertexShader]
        public FragmentInput VS(VertexGPU input)
        {
            FragmentInput output;
            output.Position = ShaderBuiltins.Mul(Ortho, new Vector4(input.Position, 0, 1));
            output.Color = input.Color;
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
