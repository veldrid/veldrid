using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("Box", "TestApp.Shaders.Box.VS", "TestApp.Shaders.Box.FS")]

namespace TestApp.Shaders
{
    public class Box
    {
        //public Matrix4x4 World;
        //public Matrix4x4 View;
        //public Matrix4x4 Projection;
        public Matrix4x4 WVP;
        public Texture2DResource SurfaceTexture;
        public SamplerResource Sampler;

        [VertexShader]
        public FragmentInput VS(VertexPositionNormalTexture input)
        {
            FragmentInput output;
            output.Position = Mul(WVP, new Vector4(input.Position, 1));
            output.TextureCoordinates = input.TextureCoordinates;
            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            return Sample(SurfaceTexture, Sampler, input.TextureCoordinates);
        }

        public struct FragmentInput
        {
            [PositionSemantic] public Vector4 Position;
            [TextureCoordinateSemantic] public Vector2 TextureCoordinates;
        }
    }
}
