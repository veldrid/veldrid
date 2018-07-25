using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="Shader"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct ShaderDescription : IEquatable<ShaderDescription>
    {
        /// <summary>
        /// The shader stage this instance describes.
        /// </summary>
        public ShaderStages Stage;

        /// <summary>
        /// An array containing the raw shader bytes.
        /// For Direct3D11 shaders, this array must contain HLSL bytecode or HLSL text.
        /// For Vulkan shaders, this array must contain SPIR-V bytecode.
        /// For OpenGL and OpenGL ES shaders, this array must contain the ASCII-encoded text of the shader code.
        /// For Metal shaders, this array must contain Metal bitcode (a "metallib" file), or UTF8-encoded Metal shading language
        /// text.
        /// </summary>
        public byte[] ShaderBytes;

        /// <summary>
        /// The name of the entry point function in the shader module to be used in this stage.
        /// </summary>
        public string EntryPoint;

        /// <summary>
        /// Indicates whether the shader should be debuggable. This flag only has an effect if <see cref="ShaderBytes"/> contains
        /// shader code that will be compiled.
        /// </summary>
        public bool Debug;

        /// <summary>
        /// Constructs a new ShaderDescription.
        /// </summary>
        /// <param name="stage">The shader stage to create.</param>
        /// <param name="shaderBytes">An array containing the raw shader bytes.</param>
        /// <param name="entryPoint">The name of the entry point function in the shader module to be used in this stage.</param>
        public ShaderDescription(ShaderStages stage, byte[] shaderBytes, string entryPoint)
        {
            Stage = stage;
            ShaderBytes = shaderBytes;
            EntryPoint = entryPoint;
            Debug = false;
        }

        /// <summary>
        /// Constructs a new ShaderDescription.
        /// </summary>
        /// <param name="stage">The shader stage to create.</param>
        /// <param name="shaderBytes">An array containing the raw shader bytes.</param>
        /// <param name="entryPoint">The name of the entry point function in the shader module to be used in this stage.</param>
        /// <param name="debug">Indicates whether the shader should be debuggable. This flag only has an effect if
        /// <paramref name="shaderBytes"/> contains shader code that will be compiled.</param>
        public ShaderDescription(ShaderStages stage, byte[] shaderBytes, string entryPoint, bool debug)
        {
            Stage = stage;
            ShaderBytes = shaderBytes;
            EntryPoint = entryPoint;
            Debug = debug;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements and if array instances are equal; false otherswise.</returns>
        public bool Equals(ShaderDescription other)
        {
            return Stage == other.Stage
                && ShaderBytes == other.ShaderBytes
                && EntryPoint.Equals(other.EntryPoint)
                && Debug.Equals(other.Debug);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                (int)Stage,
                ShaderBytes.GetHashCode(),
                EntryPoint.GetHashCode(),
                Debug.GetHashCode());
        }
    }
}
