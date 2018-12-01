namespace Veldrid
{
    /// <summary>
    /// The kind of a <see cref="BindableResource"/> object.
    /// </summary>
    public enum ResourceKind : byte
    {
        /// <summary>
        /// A <see cref="DeviceBuffer"/> accessed as a uniform buffer. A subset of a buffer can be bound using a
        /// <see cref="DeviceBufferRange"/>.
        /// </summary>
        UniformBuffer,
        /// <summary>
        /// A <see cref="DeviceBuffer"/> accessed as a read-only storage buffer. A subset of a buffer can be bound using a
        /// <see cref="DeviceBufferRange"/>.
        /// </summary>
        StructuredBufferReadOnly,
        /// <summary>
        /// A <see cref="DeviceBuffer"/> accessed as a read-write storage buffer. A subset of a buffer can be bound using a
        /// <see cref="DeviceBufferRange"/>.
        /// </summary>
        StructuredBufferReadWrite,
        /// <summary>
        /// A read-only <see cref="Texture"/>, accessed through a Texture or <see cref="TextureView"/>.
        /// <remarks>Binding a <see cref="Texture"/> to a resource slot expecting a TextureReadWrite is equivalent to binding a
        /// <see cref="TextureView"/> that covers the full mip and array layer range, with the original Texture's
        /// <see cref="PixelFormat"/>.</remarks>
        /// </summary>
        TextureReadOnly,
        /// <summary>
        /// A read-write <see cref="Texture"/>, accessed through a Texture or <see cref="TextureView"/>.
        /// </summary>
        /// <remarks>Binding a <see cref="Texture"/> to a resource slot expecting a TextureReadWrite is equivalent to binding a
        /// <see cref="TextureView"/> that covers the full mip and array layer range, with the original Texture's
        /// <see cref="PixelFormat"/>.</remarks>
        TextureReadWrite,
        /// <summary>
        /// A <see cref="Veldrid.Sampler"/>.
        /// </summary>
        Sampler,
    }
}
