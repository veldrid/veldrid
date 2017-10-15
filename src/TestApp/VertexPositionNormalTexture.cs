using ShaderGen;
using System.Numerics;

namespace TestApp
{
    public struct VertexPositionNormalTexture
    {
        [PositionSemantic]
        public Vector3 Position;
        [NormalSemantic]
        public Vector3 Normal;
        [TextureCoordinateSemantic]
        public Vector2 TextureCoordinates;

        public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoords)
        {
            Position = position;
            Normal = normal;
            TextureCoordinates = texCoords;
        }
    }
}
