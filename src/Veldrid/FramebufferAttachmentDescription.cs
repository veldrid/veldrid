using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a single attachment (color or depth) for a <see cref="Framebuffer"/>.
    /// </summary>
    public struct FramebufferAttachmentDescription : IEquatable<FramebufferAttachmentDescription>
    {
        /// <summary>
        /// The target texture to render into. For color attachments, this resource must have been created with the
        /// <see cref="TextureUsage.RenderTarget"/> flag. For depth attachments, this resource must have been created with the
        /// <see cref="TextureUsage.DepthStencil"/> flag.
        /// </summary>
        public Texture Target;
        /// <summary>
        /// The array layer to render to. This value must be less than <see cref="Texture.ArrayLayers"/> in the target
        /// <see cref="Texture"/>.
        /// </summary>
        public uint ArrayLayer;
        /// <summary>
        /// The mip level to render to. This value must be less than <see cref="Texture.MipLevels"/> in the target
        /// <see cref="Texture"/>.
        /// </summary>
        public uint MipLevel;

        /// <summary>
        /// Constructs a new FramebufferAttachmentDescription.
        /// </summary>
        /// <param name="target">The target texture to render into. For color attachments, this resource must have been created
        /// with the <see cref="TextureUsage.RenderTarget"/> flag. For depth attachments, this resource must have been created
        /// with the <see cref="TextureUsage.DepthStencil"/> flag.</param>
        /// <param name="arrayLayer">The array layer to render to. This value must be less than <see cref="Texture.ArrayLayers"/>
        /// in the target <see cref="Texture"/>.</param>
        public FramebufferAttachmentDescription(Texture target, uint arrayLayer)
            : this(target, arrayLayer, 0)
        { }

        /// <summary>
        /// Constructs a new FramebufferAttachmentDescription.
        /// </summary>
        /// <param name="target">The target texture to render into. For color attachments, this resource must have been created
        /// with the <see cref="TextureUsage.RenderTarget"/> flag. For depth attachments, this resource must have been created
        /// with the <see cref="TextureUsage.DepthStencil"/> flag.</param>
        /// <param name="arrayLayer">The array layer to render to. This value must be less than <see cref="Texture.ArrayLayers"/>
        /// in the target <see cref="Texture"/>.</param>
        /// <param name="mipLevel">The mip level to render to. This value must be less than <see cref="Texture.MipLevels"/> in
        /// the target <see cref="Texture"/>.</param>
        public FramebufferAttachmentDescription(Texture target, uint arrayLayer, uint mipLevel)
        {
#if VALIDATE_USAGE
            uint effectiveArrayLayers = target.ArrayLayers;
            if ((target.Usage & TextureUsage.Cubemap) != 0)
            {
                effectiveArrayLayers *= 6;
            }

            if (arrayLayer >= effectiveArrayLayers)
            {
                throw new VeldridException(
                    $"{nameof(arrayLayer)} must be less than {nameof(target)}.{nameof(Texture.ArrayLayers)}.");
            }
            if (mipLevel >= target.MipLevels)
            {
                throw new VeldridException(
                    $"{nameof(mipLevel)} must be less than {nameof(target)}.{nameof(Texture.MipLevels)}.");
            }
#endif
            Target = target;
            ArrayLayer = arrayLayer;
            MipLevel = mipLevel;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements and all array elements are equal; false otherswise.</returns>
        public bool Equals(FramebufferAttachmentDescription other)
        {
            return Target.Equals(other.Target) && ArrayLayer.Equals(other.ArrayLayer) && MipLevel.Equals(other.MipLevel);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(Target.GetHashCode(), ArrayLayer.GetHashCode(), MipLevel.GetHashCode());
        }
    }
}
