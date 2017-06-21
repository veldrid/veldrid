using System.Numerics;

namespace Veldrid.Graphics
{
    public struct VertexPositionColor
    {
        public const byte SizeInBytes = 28;
        public const byte ColorOffset = 12;
        public const byte ElementCount = 2;

        public readonly Vector3 Position;
        public readonly RgbaFloat Color;

        public VertexPositionColor(Vector3 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
    }
}
