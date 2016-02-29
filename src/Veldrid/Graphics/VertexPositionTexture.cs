using System.Numerics;

namespace Veldrid.Graphics
{
    public struct VertexPositionTexture
    {
        public static unsafe byte SizeInBytes = (byte)sizeof(VertexPositionTexture);
        public static unsafe byte TextureCoordinatesOffset = (byte)sizeof(Vector3);
        public const byte ElementCount = 2;

        public readonly Vector3 Position;
        public readonly Vector2 TextureCoordinates;

        public VertexPositionTexture(Vector3 position, Vector2 texCoords)
        {
            Position = position;
            TextureCoordinates = texCoords;
        }
    }
}