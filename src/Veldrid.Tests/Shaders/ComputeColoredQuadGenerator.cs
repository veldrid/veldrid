using ShaderGen;
using System.Numerics;
using System.Runtime.InteropServices;

[assembly: ComputeShaderSet("ComputeColoredQuadGenerator", "Veldrid.Tests.Shaders.ComputeColoredQuadGenerator.CS")]

namespace Veldrid.Tests.Shaders
{
    public class ComputeColoredQuadGenerator
    {
        public RWStructuredBuffer<ColoredVertex> OutputVertices;

        [ComputeShader(1, 1, 1)]
        public void CS()
        {
            OutputVertices[0].Position = new Vector2(-1f, 1f);
            OutputVertices[0].Color = new Vector4(1, 0, 0, 1);

            OutputVertices[1].Position = new Vector2(1f, 1f);
            OutputVertices[1].Color = new Vector4(1, 0, 0, 1);

            OutputVertices[2].Position = new Vector2(-1f, -1f);
            OutputVertices[2].Color = new Vector4(1, 0, 0, 1);

            OutputVertices[3].Position = new Vector2(1f, -1f);
            OutputVertices[3].Color = new Vector4(1, 0, 0, 1);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColoredVertex
    {
        [ColorSemantic]
        public Vector4 Color;
        [PositionSemantic]
        public Vector2 Position;

        private Vector2 _padding0;
    }
}
