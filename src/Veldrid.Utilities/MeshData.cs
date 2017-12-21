using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Utilities
{
    /// <summary>
    /// An object describing generic mesh data. This can be used to construct a <see cref="VertexBuffer"/> and
    /// <see cref="IndexBuffer"/>, and also exposes functionality for bounding box and sphere computation.
    /// </summary>
    public interface MeshData
    {
        /// <summary>
        /// Constructs a <see cref="VertexBuffer"/> from this <see cref="MeshData"/>.
        /// </summary>
        /// <param name="factory">The <see cref="ResourceFactory"/> to use for device resource creation.</param>
        /// <returns></returns>
        DeviceBuffer CreateVertexBuffer(ResourceFactory factory, CommandList cl);

        /// <summary>
        /// Constructs a <see cref="IndexBuffer"/> from this <see cref="MeshData"/>.
        /// </summary>
        /// <param name="factory">The <see cref="ResourceFactory"/> to use for device resource creation.</param>
        /// <returns></returns>
        DeviceBuffer CreateIndexBuffer(ResourceFactory factory, CommandList cl, out int indexCount);

        /// <summary>
        /// Gets a centered <see cref="BoundingSphere"/> which completely encapsulates the vertices of this mesh.
        /// </summary>
        /// <returns>A <see cref="BoundingSphere"/>.</returns>
        BoundingSphere GetBoundingSphere();

        /// <summary>
        /// Gets a centered, axis-aligned <see cref="BoundingBox"/> which completely encapsulates the vertices of this mesh.
        /// </summary>
        /// <returns>An axis-aligned <see cref="BoundingBox"/>.</returns>
        BoundingBox GetBoundingBox();

        /// <summary>
        /// Performs a RayCast against the vertices of this mesh.
        /// </summary>
        /// <param name="ray">The ray to use. This ray should be in object-local space.</param>
        /// <param name="distance">If the RayCast is successful, contains the distance 
        /// from the <see cref="Ray"/> origin that the hit occurred.</param>
        /// <returns>True if the <see cref="Ray"/> intersects the mesh; false otherwise</returns>
        bool RayCast(Ray ray, out float distance);

        /// <summary>
        /// Performs a RayCast against the vertices of this mesh.
        /// </summary>
        /// <param name="ray">The ray to use. This ray should be in object-local space.</param>
        /// <param name="distances">All of the distances at which the ray passes through the mesh.</param>
        /// <returns>The number of intersections.</returns>
        int RayCast(Ray ray, List<float> distances);

        /// <summary>
        /// Gets an array containing the raw vertex positions of the mesh.
        /// </summary>
        /// <returns>An array of vertex positions.</returns>
        Vector3[] GetVertexPositions();

        /// <summary>
        /// Gets an array containing the raw indices of the mesh.
        /// </summary>
        /// <returns>An array of indices.</returns>
        ushort[] GetIndices();
    }
}
