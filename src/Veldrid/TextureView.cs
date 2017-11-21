using System;

namespace Veldrid
{
    /// <summary>
    /// A bindable device resource which provides a shader with access to a sampled <see cref="Texture"/> object.
    /// See <see cref="TextureViewDescription"/>.
    /// </summary>
    public abstract class TextureView : BindableResource, IDisposable
    {
        /// <summary>
        /// The target <see cref="Texture"/> object to be sampled via this instance.
        /// </summary>
        public Texture Target { get; }

        public uint BaseMipLevel { get; }
        public uint MipLevels { get; }
        public uint BaseArrayLayer { get; }
        public uint ArrayLayers { get; }

        internal TextureView(ref TextureViewDescription description)
        {
            Target = description.Target;
            BaseMipLevel = description.BaseMipLevel;
            MipLevels = description.MipLevels;
            BaseArrayLayer = description.BaseArrayLayer;
            ArrayLayers = description.ArrayLayers;
        }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
