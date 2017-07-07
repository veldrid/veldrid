using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Veldrid.Graphics
{
    public class SimpleMeshDataProvider : MeshData
    {
        public VertexPositionNormalTexture[] Vertices { get; }
        public ushort[] Indices { get; }
        public string MaterialName { get; }

        public SimpleMeshDataProvider(VertexPositionNormalTexture[] vertices, ushort[] indices)
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
            IndexBuffer ib = factory.CreateIndexBuffer(Indices.Length * sizeof(ushort), false);
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

        public bool RayCast(Ray ray, out float distance)
        {
            distance = float.MaxValue;
            bool result = false;
            for (int i = 0; i < Indices.Length - 2; i += 3)
            {
                Vector3 v0 = Vertices[Indices[i + 0]].Position;
                Vector3 v1 = Vertices[Indices[i + 1]].Position;
                Vector3 v2 = Vertices[Indices[i + 2]].Position;

                float newDistance;
                if (ray.Intersects(ref v0, ref v1, ref v2, out newDistance))
                {
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                    }

                    result = true;
                }
            }

            return result;
        }

        public int RayCast(Ray ray, List<float> distances)
        {
            int hits = 0;
            for (int i = 0; i < Indices.Length - 2; i += 3)
            {
                Vector3 v0 = Vertices[Indices[i + 0]].Position;
                Vector3 v1 = Vertices[Indices[i + 1]].Position;
                Vector3 v2 = Vertices[Indices[i + 2]].Position;

                float newDistance;
                if (ray.Intersects(ref v0, ref v1, ref v2, out newDistance))
                {
                    hits++;
                    distances.Add(newDistance);
                }
            }

            return hits;
        }

        public Vector3[] GetVertexPositions()
        {
            return Vertices.Select(vpnt => vpnt.Position).ToArray();
        }

        public ushort[] GetIndices()
        {
            return Indices;
        }
    }
}
