using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a compute <see cref="Pipeline"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct ComputePipelineDescription : IEquatable<ComputePipelineDescription>
    {
        /// <summary>
        /// The compute <see cref="Shader"/> to be used in the Pipeline. This must be a Shader with
        /// <see cref="ShaderStages.Compute"/>.
        /// </summary>
        public Shader ComputeShader;
        /// <summary>
        /// An array of <see cref="ResourceLayout"/>, which controls the layout of shader resoruces in the <see cref="Pipeline"/>.
        /// </summary>
        public ResourceLayout[] ResourceLayouts;
        /// <summary>
        /// The X dimension of the thread group size.
        /// </summary>
        public uint ThreadGroupSizeX;
        /// <summary>
        /// The Y dimension of the thread group size.
        /// </summary>
        public uint ThreadGroupSizeY;
        /// <summary>
        /// The Z dimension of the thread group size.
        /// </summary>
        public uint ThreadGroupSizeZ;
        /// <summary>
        /// An array of <see cref="SpecializationConstant"/> used to override specialization constants in the created
        /// <see cref="Pipeline"/>. Each element in this array describes a single ID-value pair, which will be matched with the
        /// constants specified in the <see cref="Shader"/>.
        /// </summary>
        public SpecializationConstant[] Specializations;

        /// <summary>
        /// Constructs a new ComputePipelineDescription.
        /// </summary>
        /// <param name="computeShader">The compute <see cref="Shader"/> to be used in the Pipeline. This must be a Shader with
        /// <see cref="ShaderStages.Compute"/>.</param>
        /// <param name="resourceLayouts">The set of resource layouts available to the Pipeline.</param>
        /// <param name="threadGroupSizeX">The X dimension of the thread group size.</param>
        /// <param name="threadGroupSizeY">The Y dimension of the thread group size.</param>
        /// <param name="threadGroupSizeZ">The Z dimension of the thread group size.</param>
        public ComputePipelineDescription(
            Shader computeShader,
            ResourceLayout[] resourceLayouts,
            uint threadGroupSizeX,
            uint threadGroupSizeY,
            uint threadGroupSizeZ)
        {
            ComputeShader = computeShader;
            ResourceLayouts = resourceLayouts;
            ThreadGroupSizeX = threadGroupSizeX;
            ThreadGroupSizeY = threadGroupSizeY;
            ThreadGroupSizeZ = threadGroupSizeZ;
            Specializations = null;
        }

        /// <summary>
        /// Constructs a new ComputePipelineDescription.
        /// </summary>
        /// <param name="shaderStage">The compute <see cref="Shader"/> to be used in the Pipeline. This must be a Shader with
        /// <see cref="ShaderStages.Compute"/>.</param>
        /// <param name="resourceLayout">The resource layout available to the Pipeline.</param>
        /// <param name="threadGroupSizeX">The X dimension of the thread group size.</param>
        /// <param name="threadGroupSizeY">The Y dimension of the thread group size.</param>
        /// <param name="threadGroupSizeZ">The Z dimension of the thread group size.</param>
        public ComputePipelineDescription(
            Shader shaderStage,
            ResourceLayout resourceLayout,
            uint threadGroupSizeX,
            uint threadGroupSizeY,
            uint threadGroupSizeZ)
        {
            ComputeShader = shaderStage;
            ResourceLayouts = new[] { resourceLayout };
            ThreadGroupSizeX = threadGroupSizeX;
            ThreadGroupSizeY = threadGroupSizeY;
            ThreadGroupSizeZ = threadGroupSizeZ;
            Specializations = null;
        }

        /// <summary>
        /// Constructs a new ComputePipelineDescription.
        /// </summary>
        /// <param name="shaderStage">The compute <see cref="Shader"/> to be used in the Pipeline. This must be a Shader with
        /// <see cref="ShaderStages.Compute"/>.</param>
        /// <param name="resourceLayout">The resource layout available to the Pipeline.</param>
        /// <param name="threadGroupSizeX">The X dimension of the thread group size.</param>
        /// <param name="threadGroupSizeY">The Y dimension of the thread group size.</param>
        /// <param name="threadGroupSizeZ">The Z dimension of the thread group size.</param>
        /// <param name="specializations">An array of <see cref="SpecializationConstant"/> used to override specialization
        /// constants in the created <see cref="Pipeline"/>. Each element in this array describes a single ID-value pair, which
        /// will be matched with the constants specified in the <see cref="Shader"/>.</param>
        public ComputePipelineDescription(
            Shader shaderStage,
            ResourceLayout resourceLayout,
            uint threadGroupSizeX,
            uint threadGroupSizeY,
            uint threadGroupSizeZ,
            SpecializationConstant[] specializations)
        {
            ComputeShader = shaderStage;
            ResourceLayouts = new[] { resourceLayout };
            ThreadGroupSizeX = threadGroupSizeX;
            ThreadGroupSizeY = threadGroupSizeY;
            ThreadGroupSizeZ = threadGroupSizeZ;
            Specializations = specializations;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements and all array elements are equal; false otherswise.</returns>
        public bool Equals(ComputePipelineDescription other)
        {
            return ComputeShader.Equals(other.ComputeShader)
                && Util.ArrayEquals(ResourceLayouts, other.ResourceLayouts)
                && ThreadGroupSizeX.Equals(other.ThreadGroupSizeX)
                && ThreadGroupSizeY.Equals(other.ThreadGroupSizeY)
                && ThreadGroupSizeZ.Equals(other.ThreadGroupSizeZ);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                ComputeShader.GetHashCode(),
                HashHelper.Array(ResourceLayouts),
                ThreadGroupSizeX.GetHashCode(),
                ThreadGroupSizeY.GetHashCode(),
                ThreadGroupSizeZ.GetHashCode());
        }
    }
}
