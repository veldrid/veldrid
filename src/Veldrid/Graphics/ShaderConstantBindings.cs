namespace Veldrid.Graphics
{
    /// <summary>
    /// A device-specific representation of the constant buffers available to a <see cref="ShaderSet"/>.
    /// </summary>
    public interface ShaderConstantBindingSlots
    {
        /// <summary>
        /// A device-agnostic description of the constant buffers.
        /// </summary>
        ShaderConstantDescription[] Constants { get; }
    }
}
