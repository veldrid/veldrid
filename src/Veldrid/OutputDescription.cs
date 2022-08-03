using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a set of output attachments and their formats.
    /// </summary>
    public struct OutputDescription : IEquatable<OutputDescription>
    {
        /// <summary>
        /// A description of the depth attachment, or null if none exists.
        /// </summary>
        public OutputAttachmentDescription? DepthAttachment;

        /// <summary>
        /// An array of attachment descriptions, one for each color attachment. May be null or empty.
        /// </summary>
        public OutputAttachmentDescription[]? ColorAttachments;

        /// <summary>
        /// The number of samples in each target attachment.
        /// </summary>
        public TextureSampleCount SampleCount;

        /// <summary>
        /// Constructs a new <see cref="OutputDescription"/>.
        /// </summary>
        /// <param name="depthAttachment">A description of the depth attachment.</param>
        /// <param name="colorAttachments">An array of descriptions of each color attachment.</param>
        public OutputDescription(OutputAttachmentDescription? depthAttachment, params OutputAttachmentDescription[]? colorAttachments)
        {
            DepthAttachment = depthAttachment;
            ColorAttachments = colorAttachments;
            SampleCount = TextureSampleCount.Count1;
        }

        /// <summary>
        /// Constructs a new <see cref="OutputDescription"/>.
        /// </summary>
        /// <param name="depthAttachment">A description of the depth attachment.</param>
        /// <param name="colorAttachments">An array of descriptions of each color attachment.</param>
        /// <param name="sampleCount">The number of samples in each target attachment.</param>
        public OutputDescription(
            OutputAttachmentDescription? depthAttachment,
            OutputAttachmentDescription[]? colorAttachments,
            TextureSampleCount sampleCount)
        {
            DepthAttachment = depthAttachment;
            ColorAttachments = colorAttachments;
            SampleCount = sampleCount;
        }

        internal static OutputDescription CreateFromFramebuffer(Framebuffer fb)
        {
            TextureSampleCount sampleCount = 0;

            FramebufferAttachment? fbDepthAttachment = fb.DepthTarget;
            OutputAttachmentDescription? depthAttachment = null;
            if (fbDepthAttachment != null)
            {
                depthAttachment = new OutputAttachmentDescription(fbDepthAttachment.GetValueOrDefault().Target.Format);
                sampleCount = fbDepthAttachment.GetValueOrDefault().Target.SampleCount;
            }

            ReadOnlySpan<FramebufferAttachment> fbColorAttachments = fb.ColorTargets;
            OutputAttachmentDescription[] colorAttachments = new OutputAttachmentDescription[fbColorAttachments.Length];
            for (int i = 0; i < colorAttachments.Length; i++)
            {
                colorAttachments[i] = new OutputAttachmentDescription(fbColorAttachments[i].Target.Format);
                sampleCount = fbColorAttachments[i].Target.SampleCount;
            }

            return new OutputDescription(depthAttachment, colorAttachments, sampleCount);
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements and all array elements are equal; false otherswise.</returns>
        public bool Equals(OutputDescription other)
        {
            return DepthAttachment.GetValueOrDefault().Equals(other.DepthAttachment.GetValueOrDefault())
                && Util.ArrayEqualsEquatable(ColorAttachments, other.ColorAttachments)
                && SampleCount == other.SampleCount;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                DepthAttachment.GetHashCode(),
                HashHelper.Array(ColorAttachments),
                (int)SampleCount);
        }
    }
}
