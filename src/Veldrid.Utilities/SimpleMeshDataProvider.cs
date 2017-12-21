using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Veldrid.Utilities
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

        public DeviceBuffer CreateVertexBuffer(ResourceFactory factory, CommandList cl)
        {
            DeviceBuffer vb = factory.CreateBuffer(
                new BufferDescription(
                    (uint)(Vertices.Length * VertexPositionNormalTexture.SizeInBytes),
                    BufferUsage.VertexBuffer));
            cl.UpdateBuffer(vb, 0, Vertices);
            return vb;
        }

        public DeviceBuffer CreateIndexBuffer(ResourceFactory factory, CommandList cl, out int indexCount)
        {
            DeviceBuffer ib = factory.CreateBuffer(new BufferDescription((uint)(Indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(ib, 0, Indices);
            indexCount = Indices.Length;
            return ib;
        }

        public unsafe BoundingSphere GetBoundingSphere()
        {
            fixed (void* ptr = Vertices)
            {
                return BoundingSphere.CreateFromPoints((Vector3*)ptr, Vertices.Length, VertexPositionNormalTexture.SizeInBytes);
            }
        }

        public unsafe BoundingBox GetBoundingBox()
        {
            fixed (void* ptr = Vertices)
            {
                return BoundingBox.CreateFromPoints(
                    (Vector3*)ptr,
                    Vertices.Length,
                    VertexPositionNormalTexture.SizeInBytes,
                    Quaternion.Identity,
                    Vector3.Zero,
                    Vector3.One);
            }

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
