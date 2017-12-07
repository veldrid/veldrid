using System;

namespace Veldrid
{
    /// <summary>
    /// A <see cref="Pipeline"/> component describing a full set of shader stages and vertex layouts.
    /// </summary>
    public struct ShaderSetDescription : IEquatable<ShaderSetDescription>
    {
        /// <summary>
        /// An array of <see cref="VertexLayoutDescription"/> describing the set of vertex layouts understood by the
        /// <see cref="Pipeline"/>. Each element in this array describes the input layout of a single <see cref="Buffer"/>
        /// to be bound when drawing.
        /// </summary>
        public VertexLayoutDescription[] VertexLayouts;
        /// <summary>
        /// An array of <see cref="Shader"/> objects, one for each shader stage which is to be active in the
        /// <see cref="Pipeline"/>. At a minimum, every graphics Pipeline must include a Vertex and Fragment
        /// shader. All other stages are optional, but if either Tessellation stage is present, then the other must also be.
        /// </summary>
        public Shader[] Shaders;

        /// <summary>
        /// Constructs a new ShaderSetDescription.
        /// </summary>
        /// <param name="vertexLayouts">An array of <see cref="VertexLayoutDescription"/> describing the set of vertex layouts
        /// understood by the <see cref="Pipeline"/>. Each element in this array describes the input layout of a single
        /// <see cref="Buffer"/> to be bound when drawing.</param>
        /// <param name="shaders">An array of <see cref="Shader"/> objects, one for each shader stage which is to be active
        /// in the <see cref="Pipeline"/>. At a minimum, every graphics Pipeline must include a Vertex and Fragment shader. All
        /// other stages are optional, but if either Tessellation stage is present, then the other must also be.</param>
        public ShaderSetDescription(VertexLayoutDescription[] vertexLayouts, Shader[] shaders)
        {
            VertexLayouts = vertexLayouts;
            Shaders = shaders;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all array elements are equal; false otherswise.</returns>
        public bool Equals(ShaderSetDescription other)
        {
            return Util.ArrayEqualsEquatable(VertexLayouts, other.VertexLayouts);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(HashHelper.Array(VertexLayouts), HashHelper.Array(Shaders));
        }
    }
}
