using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("Grid", "Shaders.Grid.VS", "Shaders.Grid.FS")]

namespace Shaders
{
    public class Grid
    {
        public Matrix4x4 Projection;
        public Matrix4x4 View;
        public Texture2DResource GridTexture;
        public SamplerResource GridSampler;

        public struct VSInput
        {
            [PositionSemantic] public Vector3 Position;
        }

        public struct FSInput
        {
            [PositionSemantic] public Vector4 FragPosition;
            [PositionSemantic] public Vector3 WorldPosition;
        }

        [VertexShader]
        public FSInput VS(VSInput input)
        {
            FSInput output;
            output.FragPosition = Mul(Projection, Mul(View, new Vector4(input.Position, 1)));
            output.WorldPosition = input.Position;
            return output;
        }

        [FragmentShader]
        public Vector4 FS(FSInput input)
        {
            return Sample(
                GridTexture,
                GridSampler,
                new Vector2(input.WorldPosition.X, input.WorldPosition.Z) / 10.0f);
        }
    }
}
