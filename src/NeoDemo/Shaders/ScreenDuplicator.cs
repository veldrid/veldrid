using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("ScreenDuplicator", "Shaders.ScreenDuplicator.VS", "Shaders.ScreenDuplicator.FS")]

namespace Shaders
{
    public class ScreenDuplicator
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

        public struct FragmentOutput
        {
            [ColorTargetSemantic] public Vector4 ColorOut0;
            [ColorTargetSemantic] public Vector4 ColorOut1;
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
        public FragmentOutput FS(FragmentInput input)
        {
            FragmentOutput output;
            output.ColorOut0 = Saturate(Sample(SourceTexture, SourceSampler, input.TexCoords));
            output.ColorOut1 = Saturate(Sample(SourceTexture, SourceSampler, input.TexCoords) * new Vector4(1.0f, 0.7f, 0.7f, 1f));
            return output;
        }
    }
}
