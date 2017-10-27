using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo
{
    public static class CubeModel
    {
        public static readonly VertexPositionNormalTexture[] Vertices = new VertexPositionNormalTexture[]
        {
            // Top
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,-.5f),     new Vector3(0,1,0),     new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,-.5f),      new Vector3(0,1,0),     new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,.5f),       new Vector3(0,1,0),     new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,.5f),      new Vector3(0,1,0),     new Vector2(0, 1)),
            // Bottom                                                             
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,.5f),     new Vector3(0,-1,0),     new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,.5f),      new Vector3(0,-1,0),     new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,-.5f),     new Vector3(0,-1,0),     new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,-.5f),    new Vector3(0,-1,0),     new Vector2(0, 1)),
            // Left                                                               
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,-.5f),     new Vector3(-1,0,0),    new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,.5f),      new Vector3(-1,0,0),    new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,.5f),     new Vector3(-1,0,0),    new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,-.5f),    new Vector3(-1,0,0),    new Vector2(0, 1)),
            // Right                                                              
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,.5f),       new Vector3(1,0,0),     new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,-.5f),      new Vector3(1,0,0),     new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,-.5f),     new Vector3(1,0,0),     new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,.5f),      new Vector3(1,0,0),     new Vector2(0, 1)),
            // Back                                                               
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,-.5f),      new Vector3(0,0,-1),    new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,-.5f),     new Vector3(0,0,-1),    new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,-.5f),    new Vector3(0,0,-1),    new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,-.5f),     new Vector3(0,0,-1),    new Vector2(0, 1)),
            // Front                                                              
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,.5f),      new Vector3(0,0,1),     new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,.5f),       new Vector3(0,0,1),     new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,.5f),      new Vector3(0,0,1),     new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,.5f),     new Vector3(0,0,1),     new Vector2(0, 1)),
        };

        public static readonly ushort[] Indices = new ushort[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23,
        };
    }
}
