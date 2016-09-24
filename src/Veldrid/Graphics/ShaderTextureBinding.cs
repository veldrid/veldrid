using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A type of resource which allows <see cref="DeviceTexture"/> objects to be bound for use in a <see cref="Shader"/>.
    /// </summary>
    public interface ShaderTextureBinding : IDisposable
    {
        /// <summary>
        /// The <see cref="DeviceTexture"/> associated with thie binding.
        /// </summary>
        DeviceTexture BoundTexture { get; }
    }
}
