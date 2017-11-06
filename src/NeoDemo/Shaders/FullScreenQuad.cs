using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("FullScreenQuad", "Shaders.FullScreenQuad.VS", "Shaders.FullScreenQuad.FS")]

namespace Shaders
{
    public class FullScreenQuad
    {
        public struct VertexInput
        {
            [PositionSemantic] public Vector2 Position;
            [TextureCoordinateSemantic] public Vector2 TexCoords;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic] public Vector4 Position;
            [TextureCoordinateSemantic] public Vector2 TexCoords;
        }

        public Texture2DResource SourceTexture;
        public SamplerResource SourceSampler;

        [VertexShader]
        public FragmentInput VS(VertexInput input)
        {
            FragmentInput output;
            output.Position = new Vector4(input.Position.X, input.Position.Y, 0, 1);
            output.TexCoords = input.TexCoords;
            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            return Sample(SourceTexture, SourceSampler, input.TexCoords);
        }
    }
}
