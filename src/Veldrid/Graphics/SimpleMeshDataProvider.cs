using System;

namespace Veldrid.Graphics
{
    public class SimpleMeshDataProvider : MeshData
    {
        public VertexPositionNormalTexture[] Vertices { get; }
        public int[] Indices { get; }
        public string MaterialName { get; }

        public SimpleMeshDataProvider(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public VertexBuffer CreateVertexBuffer(ResourceFactory factory)
        {
            var vb = factory.CreateVertexBuffer(Vertices.Length * VertexPositionNormalTexture.SizeInBytes, false);
            vb.SetVertexData(Vertices, new VertexDescriptor(VertexPositionNormalTexture.SizeInBytes, 3, 0, IntPtr.Zero));
            return vb;
        }

        public IndexBuffer CreateIndexBuffer(ResourceFactory factory, out int indexCount)
        {
            IndexBuffer ib = factory.CreateIndexBuffer(Indices.Length * sizeof(int), false);
            ib.SetIndices(Indices);
            indexCount = Indices.Length;
            return ib;
        }

        public BoundingSphere GetBoundingSphere()
        {
            return BoundingSphere.CreateFromPoints(Vertices);
        }

        public BoundingBox GetBoundingBox()
        {
            return BoundingBox.CreateFromVertices(Vertices);
        }
    }
}
