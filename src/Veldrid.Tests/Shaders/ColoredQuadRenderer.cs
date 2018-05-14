using ShaderGen;
using System.Numerics;

[assembly: ShaderSet("ColoredQuadRenderer", "Veldrid.Tests.Shaders.ColoredQuadRenderer.VS", "Veldrid.Tests.Shaders.ColoredQuadRenderer.FS")]

namespace Veldrid.Tests.Shaders
{
    public class ColoredQuadRenderer
    {
        public StructuredBuffer<ColoredVertex> InputVertices;

        [VertexShader]
        public ColoredVertexOutput VS()
        {
            ColoredVertex input = InputVertices[ShaderBuiltins.VertexID];
            ColoredVertexOutput output;
            output.SysPosition = new Vector4(input.Position, 0, 1);
            output.Color = input.Color;
            return output;
        }

        [FragmentShader]
        public Vector4 FS(ColoredVertexOutput input)
        {
            return input.Color;
        }
    }

    public struct ColoredVertexOutput
    {
        [SystemPositionSemantic]
        public Vector4 SysPosition;
        [ColorSemantic]
        public Vector4 Color;
    }
}
