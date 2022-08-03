using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to store arbitrary image data in a specific format.
    /// See <see cref="TextureDescription"/>.
    /// </summary>
    public abstract class Texture : DeviceResource, MappableResource, IDisposable, BindableResource
    {
        private readonly object _fullTextureViewLock = new();
        private TextureView? _fullTextureView;

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
        public PixelFormat Format { get; protected set; }

        /// <summary>
        /// The total width of this instance, in texels.
        /// </summary>
        public uint Width { get; protected set; }

        /// <summary>
        /// The total height of this instance, in texels.
        /// </summary>
        public uint Height { get; protected set; }

        /// <summary>
        /// The total depth of this instance, in texels.
        /// </summary>
        public uint Depth { get; protected set; }

        /// <summary>
        /// The total number of mipmap levels in this instance.
        /// </summary>
        public uint MipLevels { get; protected set; }

        /// <summary>
        /// The total number of array layers in this instance.
        /// </summary>
        public uint ArrayLayers { get; protected set; }

        /// <summary>
        /// The usage flags given when this instance was created. This property controls how this instance is permitted to be
        /// used, and it is an error to attempt to use the Texture outside of those contexts.
        /// </summary>
        public TextureUsage Usage { get; protected set; }

        /// <summary>
        /// The <see cref="TextureType"/> of this instance.
        /// </summary>
        public TextureType Type { get; protected set; }

        /// <summary>
        /// The number of samples in this instance. If this returns any value other than <see cref="TextureSampleCount.Count1"/>,
        /// then this instance is a multipsample texture.
        /// </summary>
        public TextureSampleCount SampleCount { get; protected set; }

        /// <inheritdoc/>
        public abstract string? Name { get; set; }

        /// <summary>
        /// A bool indicating whether this instance has been disposed.
        /// </summary>
        public abstract bool IsDisposed { get; }

        internal virtual void GetSubresourceLayout(uint mipLevel, uint arrayLevel, out uint rowPitch, out uint depthPitch)
        {
            uint blockSize = FormatHelpers.IsCompressedFormat(Format) ? 4u : 1u;
            Util.GetMipDimensions(this, mipLevel, out uint mipWidth, out uint mipHeight, out _);
            uint storageWidth = Math.Max(blockSize, mipWidth);
            uint storageHeight = Math.Max(blockSize, mipHeight);
            rowPitch = FormatHelpers.GetRowPitch(storageWidth, Format);
            depthPitch = FormatHelpers.GetDepthPitch(rowPitch, storageHeight, Format);
        }

        /// <inheritdoc/>
        public virtual uint GetSizeInBytes(uint subresource)
        {
            Util.GetMipLevelAndArrayLayer(this, subresource, out uint mipLevel, out _);
            uint blockSize = FormatHelpers.IsCompressedFormat(Format) ? 4u : 1u;
            Util.GetMipDimensions(this, mipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
            uint storageWidth = Math.Max(blockSize, mipWidth);
            uint storageHeight = Math.Max(blockSize, mipHeight);
            return mipDepth * FormatHelpers.GetDepthPitch(
                FormatHelpers.GetRowPitch(storageWidth, Format),
                storageHeight,
                Format);
        }

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
