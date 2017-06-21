using System.Numerics;

namespace Veldrid.Graphics
{
    public struct VertexPositionNormalColorTexture
    {
        public const byte SizeInBytes = 48;
        public const byte NormalOffset = 12;
        public const byte ColorOffset = 24;
        public const byte TextureCoordinatesOffset = 40;
        public const byte ElementCount = 4;

        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly RgbaFloat Color;
        public readonly Vector2 TextureCoordinates;

        public VertexPositionNormalColorTexture(Vector3 position, Vector3 normal, RgbaFloat color, Vector2 texCoords)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TextureCoordinates = texCoords;
        }
    }
}
