using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to store arbitrary image data in a specific format.
    /// See <see cref="TextureDescription"/>.
    /// </summary>
    public abstract class Texture : DeviceResource, MappableResource, IDisposable, BindableResource
    {
        private readonly object _fullTextureViewLock = new object();
        private TextureView _fullTextureView;

        /// <summary>
        /// Calculates the subresource index, given a mipmap level and array layer.
        /// </summary>
        /// <param name="mipLevel">The mip level. This should be less than <see cref="MipLevels"/>.</param>
        /// <param name="arrayLayer">The array layer. This should be less than <see cref="ArrayLayers"/>.</param>
        /// <returns>The subresource index.</returns>
        public uint CalculateSubresource(uint mipLevel, uint arrayLayer)
        {
            return arrayLayer * MipLevels + mipLevel;
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
        /// The <see cref="TextureType"/> of this instance.
        /// </summary>
        public abstract TextureType Type { get; }
        /// <summary>
        /// The number of samples in this instance. If this returns any value other than <see cref="TextureSampleCount.Count1"/>,
        /// then this instance is a multipsample texture.
        /// </summary>
        public abstract TextureSampleCount SampleCount { get; }
        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public abstract string Name { get; set; }
        /// <summary>
        /// A bool indicating whether this instance has been disposed.
        /// </summary>
        public abstract bool IsDisposed { get; }

        internal TextureView GetFullTextureView(GraphicsDevice gd)
        {
            lock (_fullTextureViewLock)
            {
                if (_fullTextureView == null)
                {
                    _fullTextureView = CreateFullTextureView(gd);
                }

                return _fullTextureView;
            }
        }

        private protected virtual TextureView CreateFullTextureView(GraphicsDevice gd)
        {
            return gd.ResourceFactory.CreateTextureView(this);
        }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public virtual void Dispose()
        {
            lock (_fullTextureViewLock)
            {
                _fullTextureView?.Dispose();
            }

            DisposeCore();
        }

        private protected abstract void DisposeCore();
    }
}
