using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a single shader stage.
    /// </summary>
    public struct ShaderStageDescription : IEquatable<ShaderStageDescription>
    {
        /// <summary>
        /// The shader module.
        /// </summary>
        public Shader Shader;
        /// <summary>
        /// The name of the entry point function in the shader module to be used in this stage.
        /// </summary>
        public string EntryPoint; // Vulkan requires entry point name when a Pipeline is created with bytecode.

        /// <summary>
        /// Constructs a new ShaderStageDescription.
        /// </summary>
        /// <param name="shader">The shader module.</param>
        /// <param name="entryPoint">The name of the entry point function in the shader module to be used in this stage.</param>
        public ShaderStageDescription(Shader shader, string entryPoint)
        {
            Shader = shader;
            EntryPoint = entryPoint;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(ShaderStageDescription other)
        {
            return Shader.Equals(other.Shader) && EntryPoint.Equals(other.EntryPoint);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(Shader.GetHashCode(), EntryPoint.GetHashCode());
        }
    }
}
