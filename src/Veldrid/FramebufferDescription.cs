using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="Framebuffer"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct FramebufferDescription : IEquatable<FramebufferDescription>
    {
        /// <summary>
        /// The depth texture, which must have been created with <see cref="TextureUsage.DepthStencil"/> usage flags.
        /// May be null.
        /// </summary>
        public FramebufferAttachmentDescription? DepthTarget;

        /// <summary>
        /// An array of color textures, all of which must have been created with <see cref="TextureUsage.RenderTarget"/>
        /// usage flags. May be null or empty.
        /// </summary>
        public FramebufferAttachmentDescription[] ColorTargets;

        /// <summary>
        /// An array of color textures. May be null. If non-null, this must be the same size as
        /// <see cref="ColorTargets"/>. Each element int his array indicates a multisample resolve target for the
        /// matching array element in the <see cref="ColorTargets"/> array.
        /// </summary>
        public FramebufferAttachmentDescription[] ResolveTargets;

        internal FramebufferAttachmentDescription[] ResolveTargetsOrEmpty()
            => ResolveTargets ?? Array.Empty<FramebufferAttachmentDescription>();

        /// <summary>
        /// Constructs a new <see cref="FramebufferDescription"/>.
        /// </summary>
        /// <param name="depthTarget">The depth texture, which must have been created with
        /// <see cref="TextureUsage.DepthStencil"/> usage flags. May be null.</param>
        /// <param name="colorTargets">An array of color textures, all of which must have been created with
        /// <see cref="TextureUsage.RenderTarget"/> usage flags. May be null or empty.</param>
        public FramebufferDescription(Texture depthTarget, params Texture[] colorTargets)
            : this(depthTarget, colorTargets, Array.Empty<Texture>())
        { }

        public FramebufferDescription(Texture depthTarget, Texture[] colorTargets, Texture[] resolveTargets)
        {
            if (depthTarget != null)
            {
                DepthTarget = new FramebufferAttachmentDescription(depthTarget, 0);
            }
            else
            {
                DepthTarget = null;
            }

            ColorTargets = new FramebufferAttachmentDescription[colorTargets.Length];
            for (int i = 0; i < colorTargets.Length; i++)
            {
                ColorTargets[i] = new FramebufferAttachmentDescription(colorTargets[i], 0);
            }


            ResolveTargets = new FramebufferAttachmentDescription[resolveTargets.Length];
            for (int i = 0; i < resolveTargets.Length; i++)
            {
                ResolveTargets[i] = new FramebufferAttachmentDescription(resolveTargets[i], 0);
            }
        }

        /// <summary>
        /// Constructs a new <see cref="FramebufferDescription"/>.
        /// </summary>
        /// <param name="depthTarget">A description of the depth attachment. May be null if no depth attachment will be used.</param>
        /// <param name="colorTargets">An array of descriptions of color attachments. May be empty if no color attachments will
        /// be used.</param>
        public FramebufferDescription(
            FramebufferAttachmentDescription? depthTarget,
            FramebufferAttachmentDescription[] colorTargets)
            : this(depthTarget, colorTargets, null)
        { }

        /// <summary>
        /// Constructs a new <see cref="FramebufferDescription"/>.
        /// </summary>
        /// <param name="depthTarget">A description of the depth attachment. May be null if no depth attachment will be
        /// used.</param>
        /// <param name="colorTargets">An array of descriptions of color attachments. May be empty if no color
        /// attachments will be used.</param>
        /// <param name="resolveTargets">An array of descriptions of resolve attachments. May be empty if no color
        /// attachments will be resolved.</param>
        public FramebufferDescription(
            FramebufferAttachmentDescription? depthTarget,
            FramebufferAttachmentDescription[] colorTargets,
            FramebufferAttachmentDescription[] resolveTargets)
        {
            DepthTarget = depthTarget;
            ColorTargets = colorTargets;
            ResolveTargets = resolveTargets;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements and all array elements are equal; false otherswise.</returns>
        public bool Equals(FramebufferDescription other)
        {
            return Util.NullableEquals(DepthTarget, other.DepthTarget)
                && Util.ArrayEqualsEquatable(ColorTargets, other.ColorTargets)
                && Util.ArrayEqualsEquatable(ResolveTargets, other.ResolveTargets);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                DepthTarget.GetHashCode(),
                HashHelper.Array(ColorTargets),
                HashHelper.Array(ResolveTargets));
        }
    }
}
