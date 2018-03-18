using ShaderGen;
using System.Numerics;

[assembly: ShaderSet("TexturedPoints", "Veldrid.Tests.Shaders.TexturedPoints.VS", "Veldrid.Tests.Shaders.TexturedPoints.FS")]

namespace Veldrid.Tests.Shaders
{
    public class TexturedPoints
    {
        public struct Vertex
        {
            [PositionSemantic]
            public Vector2 Position;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic]
            public Vector4 Position;
        }

        public Matrix4x4 Ortho;
        public Texture2DResource Tex;
        public SamplerResource Smp;

        [VertexShader]
        public FragmentInput VS(Vertex input)
        {
            FragmentInput output;
            output.Position = ShaderBuiltins.Mul(Ortho, new Vector4(input.Position, 0, 1));
            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            return ShaderBuiltins.Sample(Tex, Smp, new Vector2(0.5f, 0.5f));
        }
    }
}
