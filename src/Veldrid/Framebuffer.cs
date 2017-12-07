using System;
using System.Collections.Generic;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to control which color and depth textures are rendered to.
    /// See <see cref="FramebufferDescription"/>.
    /// </summary>
    public abstract class Framebuffer : DeviceResource, IDisposable
    {
        /// <summary>
        /// Gets the depth attachment associated with this instance. May be null if no depth texture is used.
        /// </summary>
        public virtual FramebufferAttachment? DepthTarget { get; }

        /// <summary>
        /// Gets the collection of color attachments associated with this instance. May be empty.
        /// </summary>
        public virtual IReadOnlyList<FramebufferAttachment> ColorTargets { get; }

        /// <summary>
        /// Gets an <see cref="Veldrid.OutputDescription"/> which describes the number and formats of the color targets in this instance.
        /// </summary>
        public virtual OutputDescription OutputDescription { get; }

        /// <summary>
        /// Gets the width of the <see cref="Framebuffer"/>.
        /// </summary>
        public virtual uint Width { get; }

        /// <summary>
        /// Gets the height of the <see cref="Framebuffer"/>.
        /// </summary>
        public virtual uint Height { get; }

        internal Framebuffer(FramebufferAttachment? depthTexture, IReadOnlyList<FramebufferAttachment> colorTextures)
        {
            ColorTargets = colorTextures;
            DepthTarget = depthTexture;

            if (colorTextures.Count > 0)
            {
                Width = colorTextures[0].Target.Width;
                Height = colorTextures[0].Target.Height;
            }
            else if (depthTexture != null)
            {
                Width = depthTexture.Value.Target.Width;
                Height = depthTexture.Value.Target.Height;
            }

            OutputDescription = OutputDescription.CreateFromFramebuffer(this);
        }

        internal Framebuffer() { }

        internal Framebuffer(
            FramebufferAttachmentDescription? depthTargetDesc,
            IReadOnlyList<FramebufferAttachmentDescription> colorTargetDescs)
        {
            if (depthTargetDesc != null)
            {
                FramebufferAttachmentDescription depthAttachment = depthTargetDesc.Value;
                DepthTarget = new FramebufferAttachment(depthAttachment.Target, depthAttachment.ArrayLayer);
            }
            FramebufferAttachment[] colorTargets = new FramebufferAttachment[colorTargetDescs.Count];
            for (int i = 0; i < colorTargets.Length; i++)
            {
                colorTargets[i] = new FramebufferAttachment(colorTargetDescs[i].Target, colorTargetDescs[i].ArrayLayer);
            }

            ColorTargets = colorTargets;

            if (ColorTargets.Count > 0)
            {
                Width = ColorTargets[0].Target.Width;
                Height = ColorTargets[0].Target.Height;
            }
            else if (DepthTarget != null)
            {
                Width = DepthTarget.Value.Target.Width;
                Height = DepthTarget.Value.Target.Height;
            }

            OutputDescription = OutputDescription.CreateFromFramebuffer(this);
        }

        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
