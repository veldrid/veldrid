using System.Numerics;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo.Models
{
    public static class PlaneModel
    {
        public static readonly VertexPositionNormalTexture[] Vertices = new VertexPositionNormalTexture[]
        {
            new VertexPositionNormalTexture(new Vector3(-0.5f, 0, -0.5f),   Vector3.UnitY, new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(0.5f, 0, -0.5f),    Vector3.UnitY, new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(0.5f, 0, 0.5f),     Vector3.UnitY, new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(-0.5f, 0, 0.5f),    Vector3.UnitY, new Vector2(0, 1))
        };

        public static readonly ushort[] Indices = new ushort[]
        {
            0, 1, 2,
            0, 2, 3
        };
    }
}
