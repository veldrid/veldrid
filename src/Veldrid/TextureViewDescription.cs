using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="TextureView"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct TextureViewDescription : IEquatable<TextureViewDescription>
    {
        /// <summary>
        /// The desired target <see cref="Texture"/>.
        /// </summary>
        public Texture Target;
        /// <summary>
        /// The base mip level visible in the view. Must be less than <see cref="Texture.MipLevels"/>.
        /// </summary>
        public uint BaseMipLevel;
        /// <summary>
        /// The number of mip levels visible in the view.
        /// </summary>
        public uint MipLevels;
        /// <summary>
        /// The base array layer visible in the view.
        /// </summary>
        public uint BaseArrayLayer;
        /// <summary>
        /// The number of array layers visible in the view.
        /// </summary>
        public uint ArrayLayers;
        /// <summary>
        /// An optional <see cref="PixelFormat"/> which specifies how the data within <see cref="Target"/> will be viewed.
        /// If this value is null, then the created TextureView will use the same <see cref="PixelFormat"/> as the target
        /// <see cref="Texture"/>. If not null, this format must be "compatible" with the target Texture's. For uncompressed
        /// formats, the overall size and number of components in this format must be equal to the underlying format. For
        /// compressed formats, it is only possible to use the same PixelFormat or its sRGB/non-sRGB counterpart.
        /// </summary>
        public PixelFormat? Format;

        /// <summary>
        /// Constructs a new TextureViewDescription.
        /// </summary>
        /// <param name="target">The desired target <see cref="Texture"/>. This <see cref="Texture"/> must have been created
        /// with the <see cref="TextureUsage.Sampled"/> usage flag.</param>
        public TextureViewDescription(Texture target)
        {
            Target = target;
            BaseMipLevel = 0;
            MipLevels = target.MipLevels;
            BaseArrayLayer = 0;
            ArrayLayers = target.ArrayLayers;
            Format = target.Format;
        }

        /// <summary>
        /// Constructs a new TextureViewDescription.
        /// </summary>
        /// <param name="target">The desired target <see cref="Texture"/>. This <see cref="Texture"/> must have been created
        /// with the <see cref="TextureUsage.Sampled"/> usage flag.</param>
        /// <param name="format">Specifies how the data within the target Texture will be viewed.
        /// This format must be "compatible" with the target Texture's. For uncompressed formats, the overall size and number of
        /// components in this format must be equal to the underlying format. For compressed formats, it is only possible to use
        /// the same PixelFormat or its sRGB/non-sRGB counterpart.</param>
        public TextureViewDescription(Texture target, PixelFormat format)
        {
            Target = target;
            BaseMipLevel = 0;
            MipLevels = target.MipLevels;
            BaseArrayLayer = 0;
            ArrayLayers = target.ArrayLayers;
            Format = format;
        }

        /// <summary>
        /// Constructs a new TextureViewDescription.
        /// </summary>
        /// <param name="target">The desired target <see cref="Texture"/>.</param>
        /// <param name="baseMipLevel">The base mip level visible in the view. Must be less than <see cref="Texture.MipLevels"/>.
        /// </param>
        /// <param name="mipLevels">The number of mip levels visible in the view.</param>
        /// <param name="baseArrayLayer">The base array layer visible in the view.</param>
        /// <param name="arrayLayers">The number of array layers visible in the view.</param>
        public TextureViewDescription(Texture target, uint baseMipLevel, uint mipLevels, uint baseArrayLayer, uint arrayLayers)
        {
            Target = target;
            BaseMipLevel = baseMipLevel;
            MipLevels = mipLevels;
            BaseArrayLayer = baseArrayLayer;
            ArrayLayers = arrayLayers;
            Format = target.Format;
        }

        /// <summary>
        /// Constructs a new TextureViewDescription.
        /// </summary>
        /// <param name="target">The desired target <see cref="Texture"/>.</param>
        /// <param name="format">Specifies how the data within the target Texture will be viewed.
        /// This format must be "compatible" with the target Texture's. For uncompressed formats, the overall size and number of
        /// components in this format must be equal to the underlying format. For compressed formats, it is only possible to use
        /// the same PixelFormat or its sRGB/non-sRGB counterpart.</param>
        /// <param name="baseMipLevel">The base mip level visible in the view. Must be less than <see cref="Texture.MipLevels"/>.
        /// </param>
        /// <param name="mipLevels">The number of mip levels visible in the view.</param>
        /// <param name="baseArrayLayer">The base array layer visible in the view.</param>
        /// <param name="arrayLayers">The number of array layers visible in the view.</param>
        public TextureViewDescription(Texture target, PixelFormat format, uint baseMipLevel, uint mipLevels, uint baseArrayLayer, uint arrayLayers)
        {
            Target = target;
            BaseMipLevel = baseMipLevel;
            MipLevels = mipLevels;
            BaseArrayLayer = baseArrayLayer;
            ArrayLayers = arrayLayers;
            Format = target.Format;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(TextureViewDescription other)
        {
            return Target.Equals(other.Target)
                && BaseMipLevel.Equals(other.BaseMipLevel)
                && MipLevels.Equals(other.MipLevels)
                && BaseArrayLayer.Equals(other.BaseArrayLayer)
                && ArrayLayers.Equals(other.ArrayLayers)
                && Format == other.Format;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                Target.GetHashCode(),
                BaseMipLevel.GetHashCode(),
                MipLevels.GetHashCode(),
                BaseArrayLayer.GetHashCode(),
                ArrayLayers.GetHashCode(),
                Format?.GetHashCode() ?? 0);
        }
    }
}
