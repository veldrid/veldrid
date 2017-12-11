using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("ShadowDepth", "Shaders.ShadowDepth.VS", "Shaders.ShadowDepth.FS")]

namespace Shaders
{
    public class ShadowDepth
    {
        [ResourceSet(0)]
        public Matrix4x4 ViewProjection;

        [ResourceSet(1)]
        public Matrix4x4 World;

        [VertexShader]
        public FragmentInput VS(VertexInput input)
        {
            FragmentInput output;
            output.Position = Mul(ViewProjection, Mul(World, new Vector4(input.Position, 1)));
            output.Position.Y += input.TexCoord.Y * .0001f;
            return output;
        }

        [FragmentShader]
        public void FS(FragmentInput input) { }

        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;
            [NormalSemantic] public Vector3 Normal;
            [TextureCoordinateSemantic] public Vector2 TexCoord;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic] public Vector4 Position;
        }
    }
}
