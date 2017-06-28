namespace Veldrid.Graphics
{
    /// <summary>
    /// A device-specific representation of the bindable texture slots that a set of Shaders have access to.
    /// </summary>
    public interface ShaderTextureBindingSlots
    {
        /// <summary>
        /// A device-agnostic description of the texture binding slots.
        /// </summary>
        ShaderTextureInput[] TextureInputs { get; }
    }
}
