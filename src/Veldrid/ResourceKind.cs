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
        /// A <see cref="Buffer"/> accessed as a read-write storage buffer.
        /// </summary>
        StorageBufferReadWrite,
        /// <summary>
        /// A <see cref="Buffer"/> accessed as a read-only storage buffer.
        /// </summary>
        StorageBufferReadOnly,
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