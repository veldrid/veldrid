namespace Veldrid
{
    /// <summary>
    /// The kind of a <see cref="BindableResource"/> object.
    /// </summary>
    public enum ResourceKind : byte
    {
        /// <summary>
        /// A <see cref="UniformBuffer"/>.
        /// </summary>
        UniformBuffer,
        /// <summary>
        /// A <see cref="TextureView"/>.
        /// </summary>
        TextureView,
        /// <summary>
        /// A <see cref="Sampler"/>.
        /// </summary>
        Sampler,
    }
}