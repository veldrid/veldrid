using System;

namespace Veldrid
{
    public struct OutputDescription : IEquatable<OutputDescription>
    {
        public OutputAttachmentDescription? DepthAttachment;
        public OutputAttachmentDescription[] ColorAttachments;

        public OutputDescription(OutputAttachmentDescription? depthAttachment, params OutputAttachmentDescription[] colorAttachments)
        {
            DepthAttachment = depthAttachment;
            ColorAttachments = colorAttachments ?? Array.Empty<OutputAttachmentDescription>();
        }

        internal static OutputDescription CreateFromFramebuffer(Framebuffer fb)
        {
            OutputAttachmentDescription? depthAttachment = null;
            if (fb.DepthTexture != null)
            {
                depthAttachment = new OutputAttachmentDescription(fb.DepthTexture.Format);
            }
            OutputAttachmentDescription[] colorAttachments = new OutputAttachmentDescription[fb.ColorTextures.Count];
            for (int i = 0; i < colorAttachments.Length; i++)
            {
                colorAttachments[i] = new OutputAttachmentDescription(fb.ColorTextures[i].Format);
            }

            return new OutputDescription(depthAttachment, colorAttachments);
        }

        public bool Equals(OutputDescription other)
        {
            return DepthAttachment.GetValueOrDefault().Equals(other.DepthAttachment.GetValueOrDefault())
                && Util.ArrayEqualsEquatable(ColorAttachments, other.ColorAttachments);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(DepthAttachment.GetHashCode(), HashHelper.Array(ColorAttachments));
        }
    }
}