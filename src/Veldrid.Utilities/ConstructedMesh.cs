using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace Veldrid.Utilities
{
    /// <summary>
    /// An object describing generic mesh data. This can be used to construct a vertex buffer and
    /// index buffer, and also exposes functionality for bounding box and sphere computation.
    /// </summary>
    public abstract class ConstructedMesh
    {
        /// <summary>
        /// The vertices of the mesh.
        /// </summary>
        public VertexPositionNormalTexture[] Vertices { get; }

        /// <summary>
        /// The name of the <see cref="MaterialDefinition"/> associated with this mesh.
        /// </summary>
        public string? MaterialName { get; }

        public abstract int IndexCount { get; }

        public abstract IndexFormat IndexFormat { get; }

        public ConstructedMesh(VertexPositionNormalTexture[] vertices, string? materialName)
        {
            Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            MaterialName = materialName;
        }

        /// <summary>
        /// Constructs a <see cref="VertexBuffer"/> from this <see cref="MeshData"/>.
        /// </summary>
        /// <param name="factory">The <see cref="ResourceFactory"/> to use for device resource creation.</param>
        /// <returns></returns>
        public DeviceBuffer CreateVertexBuffer(ResourceFactory factory, CommandList cl)
        {
            DeviceBuffer vb = factory.CreateBuffer(
                new BufferDescription((uint)Vertices.Length * (uint)Unsafe.SizeOf<VertexPositionNormalTexture>(), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(vb, 0, Vertices);
            return vb;
        }

        /// <summary>
        /// Constructs a <see cref="IndexBuffer"/> from this <see cref="MeshData"/>.
        /// </summary>
        /// <param name="factory">The <see cref="ResourceFactory"/> to use for device resource creation.</param>
        /// <returns></returns>
        public abstract DeviceBuffer CreateIndexBuffer(ResourceFactory factory, CommandList cl);

        /// <summary>
        /// Gets a centered <see cref="BoundingSphere"/> which completely encapsulates the vertices of this mesh.
        /// </summary>
        /// <returns>A <see cref="BoundingSphere"/>.</returns>
        public BoundingSphere GetBoundingSphere()
        {
            return BoundingSphere.CreateFromPoints(
                MemoryMarshal.AsBytes(Vertices.AsSpan()),
                Unsafe.SizeOf<VertexPositionNormalTexture>());
        }

        /// <summary>
        /// Gets a centered, axis-aligned <see cref="BoundingBox"/> which completely encapsulates the vertices of this mesh.
        /// </summary>
        /// <returns>An axis-aligned <see cref="BoundingBox"/>.</returns>
        public BoundingBox GetBoundingBox()
        {
            return BoundingBox.CreateFromPoints(
                MemoryMarshal.AsBytes(Vertices.AsSpan()),
                Unsafe.SizeOf<VertexPositionNormalTexture>(),
                Quaternion.Identity,
                Vector3.Zero,
                Vector3.One);
        }

        /// <summary>
        /// Performs a ray cast against the vertices of this mesh.
        /// </summary>
        /// <param name="ray">The ray to use. This ray should be in object-local space.</param>
        /// <param name="any">
        /// Whether to break on the first intersection.
        /// May make <paramref name="distance"/> incorrect.
        /// </param>
        /// <param name="distance">
        /// If the ray cast is successful, contains the distance 
        /// from the <see cref="Ray"/> origin that the hit occurred.
        /// May be incorrect if <paramref name="any"/> is true.
        /// </param>
        /// <returns>True if the <see cref="Ray"/> intersects the mesh; false otherwise.</returns>
        public abstract bool RayCast(Ray ray, bool any, out float distance);

        /// <summary>
        /// Fills a span with the raw vertex positions of the mesh.
        /// </summary>
        /// <param name="destination">The vertex destination.</param>
        public void GetVertexPositions(Span<Vector3> destination)
        {
            ReadOnlySpan<VertexPositionNormalTexture> src = Vertices.AsSpan(0, destination.Length);
            for (int i = 0; i < src.Length; i++)
                destination[i] = src[i].Position;
        }

        /// <summary>
        /// Gets an array containing the raw vertex positions of the mesh.
        /// </summary>
        /// <returns>An array of vertex positions.</returns>
        public Vector3[] GetVertexPositions()
        {
            Vector3[] array = new Vector3[Vertices.Length];
            GetVertexPositions(array);
            return array;
        }
    }
}
