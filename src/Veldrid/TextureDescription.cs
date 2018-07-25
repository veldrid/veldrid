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
        /// The type of Texture to create.
        /// </summary>
        public TextureType Type;
        /// <summary>
        /// The number of samples. If equal to <see cref="TextureSampleCount.Count1"/>, this instance does not describe a
        /// multisample <see cref="Texture"/>.
        /// </summary>
        public TextureSampleCount SampleCount;

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
        /// <param name="type">The type of Texture to create.</param>
        public TextureDescription(
            uint width,
            uint height,
            uint depth,
            uint mipLevels,
            uint arrayLayers,
            PixelFormat format,
            TextureUsage usage,
            TextureType type)
        {
            Width = width;
            Height = height;
            Depth = depth;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            Format = format;
            Usage = usage;
            SampleCount = TextureSampleCount.Count1;
            Type = type;
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
        /// <param name="type">The type of Texture to create.</param>
        /// <param name="sampleCount">The number of samples. If any other value than <see cref="TextureSampleCount.Count1"/> is
        /// provided, then this describes a multisample texture.</param>
        public TextureDescription(
            uint width,
            uint height,
            uint depth,
            uint mipLevels,
            uint arrayLayers,
            PixelFormat format,
            TextureUsage usage,
            TextureType type,
            TextureSampleCount sampleCount)
        {
            Width = width;
            Height = height;
            Depth = depth;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            Format = format;
            Usage = usage;
            Type = type;
            SampleCount = sampleCount;
        }

        /// <summary>
        /// Creates a description for a non-multisampled 1D Texture.
        /// </summary>
        /// <param name="width">The total width, in texels.</param>
        /// <param name="mipLevels">The number of mipmap levels.</param>
        /// <param name="arrayLayers">The number of array layers.</param>
        /// <param name="format">The format of individual texture elements.</param>
        /// <param name="usage">Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader,
        /// then <see cref="TextureUsage.Sampled"/> must be included. If the Texture will be used as a depth target in a
        /// <see cref="Framebuffer"/>, then <see cref="TextureUsage.DepthStencil"/> must be included. If the Texture will be used
        /// as a color target in a <see cref="Framebuffer"/>, then <see cref="TextureUsage.RenderTarget"/> must be included.
        /// </param>
        /// <returns>A new TextureDescription for a non-multisampled 1D Texture.</returns>
        public static TextureDescription Texture1D(
            uint width,
            uint mipLevels,
            uint arrayLayers,
            PixelFormat format,
            TextureUsage usage)
        {
            return new TextureDescription(
                width,
                1,
                1,
                mipLevels,
                arrayLayers,
                format,
                usage,
                TextureType.Texture1D,
                TextureSampleCount.Count1);
        }

        /// <summary>
        /// Creates a description for a non-multisampled 2D Texture.
        /// </summary>
        /// <param name="width">The total width, in texels.</param>
        /// <param name="height">The total height, in texels.</param>
        /// <param name="mipLevels">The number of mipmap levels.</param>
        /// <param name="arrayLayers">The number of array layers.</param>
        /// <param name="format">The format of individual texture elements.</param>
        /// <param name="usage">Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader,
        /// then <see cref="TextureUsage.Sampled"/> must be included. If the Texture will be used as a depth target in a
        /// <see cref="Framebuffer"/>, then <see cref="TextureUsage.DepthStencil"/> must be included. If the Texture will be used
        /// as a color target in a <see cref="Framebuffer"/>, then <see cref="TextureUsage.RenderTarget"/> must be included.
        /// If the Texture will be used as a 2D cubemap, then <see cref="TextureUsage.Cubemap"/> must be included.</param>
        /// <returns>A new TextureDescription for a non-multisampled 2D Texture.</returns>
        public static TextureDescription Texture2D(
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            PixelFormat format,
            TextureUsage usage)
        {
            return new TextureDescription(
                width,
                height,
                1,
                mipLevels,
                arrayLayers,
                format,
                usage,
                TextureType.Texture2D,
                TextureSampleCount.Count1);
        }

        /// <summary>
        /// Creates a description for a 2D Texture.
        /// </summary>
        /// <param name="width">The total width, in texels.</param>
        /// <param name="height">The total height, in texels.</param>
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
        /// <returns>A new TextureDescription for a 2D Texture.</returns>
        public static TextureDescription Texture2D(
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            PixelFormat format,
            TextureUsage usage,
            TextureSampleCount sampleCount)
        {
            return new TextureDescription(
                width,
                height,
                1,
                mipLevels,
                arrayLayers,
                format,
                usage,
                TextureType.Texture2D,
                sampleCount);
        }

        /// <summary>
        /// Creates a description for a 3D Texture.
        /// </summary>
        /// <param name="width">The total width, in texels.</param>
        /// <param name="height">The total height, in texels.</param>
        /// <param name="depth">The total depth, in texels.</param>
        /// <param name="mipLevels">The number of mipmap levels.</param>
        /// <param name="format">The format of individual texture elements.</param>
        /// <param name="usage">Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader,
        /// then <see cref="TextureUsage.Sampled"/> must be included. If the Texture will be used as a depth target in a
        /// <see cref="Framebuffer"/>, then <see cref="TextureUsage.DepthStencil"/> must be included. If the Texture will be used
        /// as a color target in a <see cref="Framebuffer"/>, then <see cref="TextureUsage.RenderTarget"/> must be included.</param>
        /// <returns>A new TextureDescription for a 3D Texture.</returns>
        public static TextureDescription Texture3D(
            uint width,
            uint height,
            uint depth,
            uint mipLevels,
            PixelFormat format,
            TextureUsage usage)
        {
            return new TextureDescription(
                width,
                height,
                depth,
                mipLevels,
                1,
                format,
                usage,
                TextureType.Texture3D,
                TextureSampleCount.Count1);
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
                && Type == other.Type
                && SampleCount == other.SampleCount;
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
                (int)Format,
                (int)Usage,
                (int)Type,
                (int)SampleCount);
        }
    }
}
