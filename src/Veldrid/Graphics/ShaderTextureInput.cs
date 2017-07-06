namespace Veldrid.Graphics
{
    /// <summary>
    /// Describes a texture input to a shader.
    /// </summary>
    public class ShaderTextureInput
    {
        /// <summary>
        /// The slot of the texture or sampler.
        /// </summary>
        public int Slot { get; }
        /// <summary>
        /// The name of the texture or sampler in the shader.
        /// </summary>
        public string Name { get; }

        public ShaderTextureInput(int slot, string name)
        {
            Slot = slot;
            Name = name;
        }
    }
}
