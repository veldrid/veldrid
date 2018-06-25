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
        /// <see cref="Pipeline"/>. Each element in this array describes the input layout of a single <see cref="DeviceBuffer"/>
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
        /// An array of <see cref="SpecializationConstant"/> used to override specialization constants in the created
        /// <see cref="Pipeline"/>. Each element in this array describes a single ID-value pair, which will be matched with the
        /// constants specified in each <see cref="Shader"/>.
        /// </summary>
        public SpecializationConstant[] Specializations;

        /// <summary>
        /// Constructs a new ShaderSetDescription.
        /// </summary>
        /// <param name="vertexLayouts">An array of <see cref="VertexLayoutDescription"/> describing the set of vertex layouts
        /// understood by the <see cref="Pipeline"/>. Each element in this array describes the input layout of a single
        /// <see cref="DeviceBuffer"/> to be bound when drawing.</param>
        /// <param name="shaders">An array of <see cref="Shader"/> objects, one for each shader stage which is to be active
        /// in the <see cref="Pipeline"/>. At a minimum, every graphics Pipeline must include a Vertex and Fragment shader. All
        /// other stages are optional, but if either Tessellation stage is present, then the other must also be.</param>
        public ShaderSetDescription(VertexLayoutDescription[] vertexLayouts, Shader[] shaders)
        {
            VertexLayouts = vertexLayouts;
            Shaders = shaders;
            Specializations = null;
        }

        /// <summary>
        /// Constructs a new ShaderSetDescription.
        /// </summary>
        /// <param name="vertexLayouts">An array of <see cref="VertexLayoutDescription"/> describing the set of vertex layouts
        /// understood by the <see cref="Pipeline"/>. Each element in this array describes the input layout of a single
        /// <see cref="DeviceBuffer"/> to be bound when drawing.</param>
        /// <param name="shaders">An array of <see cref="Shader"/> objects, one for each shader stage which is to be active
        /// in the <see cref="Pipeline"/>. At a minimum, every graphics Pipeline must include a Vertex and Fragment shader. All
        /// other stages are optional, but if either Tessellation stage is present, then the other must also be.</param>
        /// <param name="specializations">An array of <see cref="SpecializationConstant"/> used to override specialization
        /// constants in the created <see cref="Pipeline"/>. Each element in this array describes a single ID-value pair, which
        /// will be matched with the constants specified in each <see cref="Shader"/>.</param>
        public ShaderSetDescription(
            VertexLayoutDescription[] vertexLayouts,
            Shader[] shaders,
            SpecializationConstant[] specializations)
        {
            VertexLayouts = vertexLayouts;
            Shaders = shaders;
            Specializations = specializations;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all array elements are equal; false otherswise.</returns>
        public bool Equals(ShaderSetDescription other)
        {
            return Util.ArrayEqualsEquatable(VertexLayouts, other.VertexLayouts)
                && Util.ArrayEquals(Shaders, other.Shaders)
                && Util.ArrayEqualsEquatable(Specializations, other.Specializations);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                HashHelper.Array(VertexLayouts),
                HashHelper.Array(Shaders),
                HashHelper.Array(Specializations));
        }
    }
}
