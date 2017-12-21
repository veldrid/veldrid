namespace Veldrid
{
    /// <summary>
    /// The kind of a <see cref="BindableResource"/> object.
    /// </summary>
    public enum ResourceKind : byte
    {
        /// <summary>
        /// A <see cref="DeviceBuffer"/> accessed as a uniform buffer.
        /// </summary>
        UniformBuffer,
        /// <summary>
        /// A <see cref="DeviceBuffer"/> accessed as a read-only storage buffer.
        /// </summary>
        StructuredBufferReadOnly,
        /// <summary>
        /// A <see cref="DeviceBuffer"/> accessed as a read-write storage buffer.
        /// </summary>
        StructuredBufferReadWrite,
        /// <summary>
        /// A read-only <see cref="Texture"/>, accessed through a <see cref="TextureView"/>.
        /// </summary>
        TextureReadOnly,
        /// <summary>
        /// A read-write <see cref="Texture"/>, accessed through a <see cref="TextureView"/>.
        /// </summary>
        TextureReadWrite,
        /// <summary>
        /// A <see cref="Veldrid.Sampler"/>.
        /// </summary>
        Sampler,
    }
}