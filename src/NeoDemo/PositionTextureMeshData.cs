using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo
{
    public static partial class PrimitiveShapes
    {
        internal class PositionTextureMeshData : MeshData
        {
            public readonly VertexPositionTexture[] Vertices;
            public readonly ushort[] Indices;

            public PositionTextureMeshData(VertexPositionTexture[] vertices, ushort[] indices)
            {
                Vertices = vertices;
                Indices = indices;
            }

            public DeviceBuffer CreateIndexBuffer(ResourceFactory factory, CommandList cl, out int indexCount)
            {
                DeviceBuffer ret = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
                cl.UpdateBuffer(ret, 0, Indices);
                indexCount = Indices.Length;
                return ret;
            }

            public DeviceBuffer CreateVertexBuffer(ResourceFactory factory, CommandList cl)
            {
                DeviceBuffer ret = factory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
                cl.UpdateBuffer(ret, 0, Vertices);
                return ret;
            }

            public unsafe BoundingBox GetBoundingBox()
            {
                fixed (VertexPositionTexture* vertexPtr = &Vertices[0])
                {
                    Vector3* positionPtr = (Vector3*)vertexPtr;
                    return BoundingBox.CreateFromPoints(
                        positionPtr,
                        Vertices.Length,
                        VertexPositionTexture.SizeInBytes,
                        Quaternion.Identity,
                        Vector3.Zero,
                        Vector3.One);
                }
            }

            public unsafe BoundingSphere GetBoundingSphere()
            {
                fixed (VertexPositionTexture* vertexPtr = &Vertices[0])
                {
                    Vector3* positionPtr = (Vector3*)vertexPtr;
                    return BoundingSphere.CreateFromPoints(positionPtr, Vertices.Length, VertexPositionTexture.SizeInBytes);
                }
            }

            public ushort[] GetIndices()
            {
                return Indices;
            }

            public Vector3[] GetVertexPositions()
            {
                return Vertices.Select(vpt => vpt.Position).ToArray();
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

                    if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
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

                    if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
                    {
                        hits++;
                        distances.Add(newDistance);
                    }
                }

                return hits;
            }
        }
    }
}
