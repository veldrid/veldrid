using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Utilities
{
    /// <summary>
    /// A standalone mesh created from information from an <see cref="ObjFile"/>.
    /// </summary>
    public class ConstructedMesh16 : ConstructedMesh
    {
        /// <summary>
        /// The the first index array of the mesh.
        /// </summary>
        public ushort[] Indices { get; }

        public override int IndexCount => Indices.Length;

        public override IndexFormat IndexFormat => IndexFormat.UInt16;

        /// <summary>
        /// Constructs a new <see cref="ConstructedMesh16"/>.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="indices">The indices.</param>
        /// <param name="materialName">The name of the associated MTL <see cref="MaterialDefinition"/>.</param>
        public ConstructedMesh16(VertexPositionNormalTexture[] vertices, ushort[] indices, string? materialName) :
            base(vertices, materialName)
        {
            Indices = indices ?? throw new ArgumentNullException(nameof(indices));
        }

        public override DeviceBuffer CreateIndexBuffer(ResourceFactory factory, CommandList cl)
        {
            DeviceBuffer ib = factory.CreateBuffer(new BufferDescription((uint)Indices.Length * sizeof(ushort), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(ib, 0, Indices);
            return ib;
        }

        public override bool RayCast(Ray ray, bool any, out float distance)
        {
            distance = float.MaxValue;
            bool result = false;
            for (int i = 0; i < Indices.Length - 2; i += 3)
            {
                Vector3 v0 = Vertices[Indices[i + 0]].Position;
                Vector3 v1 = Vertices[Indices[i + 1]].Position;
                Vector3 v2 = Vertices[Indices[i + 2]].Position;

                if (ray.Intersects(v0, v1, v2, out float newDistance))
                {
                    if (newDistance < distance)
                    {
                        distance = newDistance;

                        if (any)
                        {
                            return true;
                        }
                    }

                    result = true;
                }
            }

            return result;
        }

        public RayEnumerator RayCast(Ray ray)
        {
            return new RayEnumerator(this, ray);
        }

        public struct RayEnumerator : IEnumerator<float>
        {
            private int _indexOffset;

            public ConstructedMesh16 Mesh { get; }
            public Ray Ray { get; }

            public float Current { get; private set; }
            object? IEnumerator.Current => Current;

            public RayEnumerator(ConstructedMesh16 mesh, Ray ray)
            {
                Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
                Ray = ray;
                Current = default;
                _indexOffset = 0;
            }

            public bool MoveNext()
            {
                VertexPositionNormalTexture[] vertices = Mesh.Vertices;
                ushort[] indices = Mesh.Indices;

                for (; _indexOffset < indices.Length - 2; _indexOffset += 3)
                {
                    Vector3 v0 = vertices[indices[_indexOffset + 0]].Position;
                    Vector3 v1 = vertices[indices[_indexOffset + 1]].Position;
                    Vector3 v2 = vertices[indices[_indexOffset + 2]].Position;

                    if (Ray.Intersects(v0, v1, v2, out float distance))
                    {
                        Current = distance;
                        return true;
                    }
                }

                Current = default;
                return false;
            }

            public void Reset()
            {
                Current = default;
                _indexOffset = 0;
            }

            public RayEnumerator GetEnumerator()
            {
                return this;
            }

            public void Dispose()
            {
            }
        }
    }
}
