namespace Veldrid
{
    /// <summary>
    /// The kind of a <see cref="BindableResource"/> object.
    /// </summary>
    public enum ResourceKind : byte
    {
        /// <summary>
        /// A <see cref="Veldrid.UniformBuffer"/>.
        /// </summary>
        UniformBuffer,
        /// <summary>
        /// A <see cref="Veldrid.TextureView"/>.
        /// </summary>
        TextureView,
        /// <summary>
        /// A <see cref="Veldrid.Sampler"/>.
        /// </summary>
        Sampler,
    }
}