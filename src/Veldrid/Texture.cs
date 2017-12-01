using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to store arbitrary image data in a specific format.
    /// See <see cref="TextureDescription"/>.
    /// </summary>
    public abstract class Texture : DeviceResource, MappableResource, IDisposable
    {
        public uint ComputeSubresource(uint mipLevel, uint arrayLayer)
        {
            return Util.GetSubresourceIndex(this, mipLevel, arrayLayer);
        }

        /// <summary>
        /// The format of individual texture elements stored in this instance.
        /// </summary>
        public abstract PixelFormat Format { get; }
        /// <summary>
        /// The total width of this instance, in texels.
        /// </summary>
        public abstract uint Width { get; }
        /// <summary>
        /// The total height of this instance, in texels.
        /// </summary>
        public abstract uint Height { get; }
        /// <summary>
        /// The total depth of this instance, in texels.
        /// </summary>
        public abstract uint Depth { get; }
        /// <summary>
        /// The total number of mipmap levels in this instance.
        /// </summary>
        public abstract uint MipLevels { get; }
        /// <summary>
        /// The total number of array layers in this instance.
        /// </summary>
        public abstract uint ArrayLayers { get; }
        /// <summary>
        /// The usage flags given when this instance was created. This property controls how this instance is permitted to be
        /// used, and it is an error to attempt to use the Texture outside of those contexts.
        /// </summary>
        public abstract TextureUsage Usage { get; }
        /// <summary>
        /// The number of samples in this instance. If this returns any value other than <see cref="TextureSampleCount.Count1"/>,
        /// then this instance is a multipsample texture.
        /// </summary>
        public abstract TextureSampleCount SampleCount { get; }
        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
