using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("Skybox", "Shaders.Skybox.VS", "Shaders.Skybox.FS")]

namespace Shaders
{
    public class Skybox
    {
        public Matrix4x4 Projection;
        public Matrix4x4 View;
        public TextureCubeResource CubeTexture;
        public SamplerResource CubeSampler;

        public struct VSInput
        {
            [PositionSemantic] public Vector3 Position;
        }

        public struct FSInput
        {
            [PositionSemantic] public Vector4 Position;
            [TextureCoordinateSemantic] public Vector3 TexCoord;
        }

        [VertexShader]
        public FSInput VS(VSInput input)
        {
            FSInput output;
            var pos = Mul(Projection, Mul(View, new Vector4(input.Position, 1.0f)));
            output.Position = new Vector4(pos.X, pos.Y, pos.W, pos.W);
            output.TexCoord = input.Position;
            return output;
        }

        [FragmentShader]
        public Vector4 FS(FSInput input)
        {
            return Sample(
                CubeTexture,
                CubeSampler,
                input.TexCoord);
        }
    }
}
