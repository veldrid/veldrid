namespace Veldrid
{
    /// <summary>
    /// The kind of a <see cref="BindableResource"/> object.
    /// </summary>
    public enum ResourceKind : byte
    {
        /// <summary>
        /// A <see cref="Buffer"/> accessed as a uniform buffer.
        /// </summary>
        UniformBuffer,
        /// <summary>
        /// A <see cref="Buffer"/> accessed as a storage buffer.
        /// </summary>
        StorageBuffer,
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