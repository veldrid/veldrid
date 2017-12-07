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
        /// Constructs a new ComputePipelineDescription.
        /// </summary>
        /// <param name="computeShader">The compute <see cref="Shader"/> to be used in the Pipeline. This must be a Shader with
        /// <see cref="ShaderStages.Compute"/>.</param>
        /// <param name="resourceLayouts">The set of resource layouts available to the Pipeline.</param>
        public ComputePipelineDescription(Shader computeShader, ResourceLayout[] resourceLayouts)
        {
            ComputeShader = computeShader;
            ResourceLayouts = resourceLayouts;
        }

        /// <summary>
        /// Constructs a new ComputePipelineDescription.
        /// </summary>
        /// <param name="shaderStage">The compute <see cref="Shader"/> to be used in the Pipeline. This must be a Shader with
        /// <see cref="ShaderStages.Compute"/>.</param>
        /// <param name="resourceLayout">The resource layout available to the Pipeline.</param>
        public ComputePipelineDescription(Shader shaderStage, ResourceLayout resourceLayout)
        {
            ComputeShader = shaderStage;
            ResourceLayouts = new[] { resourceLayout };
        }


        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements and all array elements are equal; false otherswise.</returns>
        public bool Equals(ComputePipelineDescription other)
        {
            return ComputeShader.Equals(other.ComputeShader) && Util.ArrayEquals(ResourceLayouts, other.ResourceLayouts);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(ComputeShader.GetHashCode(), HashHelper.Array(ResourceLayouts));
        }
    }
}
