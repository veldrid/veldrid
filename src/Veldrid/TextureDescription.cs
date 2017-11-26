using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="Texture"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct TextureDescription : IEquatable<TextureDescription>
    {
        /// <summary>
        /// The total width, in texels.
        /// </summary>
        public uint Width;
        /// <summary>
        /// The total height, in texels.
        /// </summary>
        public uint Height;
        /// <summary>
        /// The total depth, in texels.
        /// </summary>
        public uint Depth;
        /// <summary>
        /// The number of mipmap levels.
        /// </summary>
        public uint MipLevels;
        /// <summary>
        /// The number of array layers.
        /// </summary>
        public uint ArrayLayers;
        /// <summary>
        /// The format of individual texture elements.
        /// </summary>
        public PixelFormat Format;
        /// <summary>
        /// Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader, then
        /// <see cref="TextureUsage.Sampled"/> must be included. If the Texture will be used as a depth target in a
        /// <see cref="Framebuffer"/>, then <see cref="TextureUsage.DepthStencil"/> must be included. If the Texture will be used
        /// as a color target in a <see cref="Framebuffer"/>, then <see cref="TextureUsage.RenderTarget"/> must be included.
        /// If the Texture will be used as a 2D cubemap, then <see cref="TextureUsage.Cubemap"/> must be included.
        /// </summary>
        public TextureUsage Usage;
        /// <summary>
        /// The number of samples. If equal to <see cref="TextureSampleCount.Count1"/>, this instance does not describe a
        /// multisample <see cref="Texture"/>.
        /// </summary>
        public TextureSampleCount SampleCount;
        /// <summary>
        /// Indicates whether this Texture is optimized for frequent updates. Dynamic Textures are able to be mapped into
        /// CPU-visible memory using the <see cref="GraphicsDevice.Map(MappableResource, uint, uint)"/> method.
        /// </summary>
        public bool Dynamic;

        /// <summary>
        /// Contsructs a new TextureDescription describing a non-multisampled <see cref="Texture"/>.
        /// </summary>
        /// <param name="width">The total width, in texels.</param>
        /// <param name="height">The total height, in texels.</param>
        /// <param name="depth">The total depth, in texels.</param>
        /// <param name="mipLevels">The number of mipmap levels.</param>
        /// <param name="arrayLayers">The number of array layers.</param>
        /// <param name="format">The format of individual texture elements.</param>
        /// <param name="usage">Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader,
        /// then <see cref="TextureUsage.Sampled"/> must be included. If the Texture will be used as a depth target in a
        /// <see cref="Framebuffer"/>, then <see cref="TextureUsage.DepthStencil"/> must be included. If the Texture will be used
        /// as a color target in a <see cref="Framebuffer"/>, then <see cref="TextureUsage.RenderTarget"/> must be included.
        /// If the Texture will be used as a 2D cubemap, then <see cref="TextureUsage.Cubemap"/> must be included.</param>
        public TextureDescription(
            uint width,
            uint height,
            uint depth,
            uint mipLevels,
            uint arrayLayers,
            PixelFormat format,
            TextureUsage usage)
        {
            Width = width;
            Height = height;
            Depth = depth;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            Format = format;
            Usage = usage;
            SampleCount = TextureSampleCount.Count1;
            Dynamic = false;
        }

        /// <summary>
        /// Contsructs a new TextureDescription.
        /// </summary>
        /// <param name="width">The total width, in texels.</param>
        /// <param name="height">The total height, in texels.</param>
        /// <param name="depth">The total depth, in texels.</param>
        /// <param name="mipLevels">The number of mipmap levels.</param>
        /// <param name="arrayLayers">The number of array layers.</param>
        /// <param name="format">The format of individual texture elements.</param>
        /// <param name="usage">Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader,
        /// then <see cref="TextureUsage.Sampled"/> must be included. If the Texture will be used as a depth target in a
        /// <see cref="Framebuffer"/>, then <see cref="TextureUsage.DepthStencil"/> must be included. If the Texture will be used
        /// as a color target in a <see cref="Framebuffer"/>, then <see cref="TextureUsage.RenderTarget"/> must be included.
        /// If the Texture will be used as a 2D cubemap, then <see cref="TextureUsage.Cubemap"/> must be included.</param>
        /// <param name="sampleCount">The number of samples. If any other value than <see cref="TextureSampleCount.Count1"/> is
        /// provided, then this describes a multisample texture.</param>
        /// <param name="dynamic">Indicates whether this Texture is optimized for frequent updates. Dynamic Textures are able to
        /// be mapped into CPU-visible memory using the <see cref="GraphicsDevice.Map(MappableResource, uint, uint)"/> method.
        /// </param>
        public TextureDescription(
            uint width,
            uint height,
            uint depth,
            uint mipLevels,
            uint arrayLayers,
            PixelFormat format,
            TextureUsage usage,
            TextureSampleCount sampleCount,
            bool dynamic)
        {
            Width = width;
            Height = height;
            Depth = depth;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            Format = format;
            Usage = usage;
            SampleCount = sampleCount;
            Dynamic = dynamic;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(TextureDescription other)
        {
            return Width.Equals(other.Width)
                && Height.Equals(other.Height)
                && Depth.Equals(other.Depth)
                && MipLevels.Equals(other.MipLevels)
                && ArrayLayers.Equals(other.ArrayLayers)
                && Format == other.Format
                && Usage == other.Usage
                && Dynamic == other.Dynamic;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                Width.GetHashCode(),
                Height.GetHashCode(),
                Depth.GetHashCode(),
                MipLevels.GetHashCode(),
                ArrayLayers.GetHashCode(),
                Format.GetHashCode(),
                Usage.GetHashCode(),
                Dynamic.GetHashCode());
        }
    }
}