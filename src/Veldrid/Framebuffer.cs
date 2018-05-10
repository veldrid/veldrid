using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        /// Gets an <see cref="Veldrid.OutputDescription"/> which describes the number and formats of the depth and color targets
        /// in this instance.
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

        internal Framebuffer() { }

        internal Framebuffer(
            FramebufferAttachmentDescription? depthTargetDesc,
            IReadOnlyList<FramebufferAttachmentDescription> colorTargetDescs)
        {
            if (depthTargetDesc != null)
            {
                FramebufferAttachmentDescription depthAttachment = depthTargetDesc.Value;
                DepthTarget = new FramebufferAttachment(
                    depthAttachment.Target,
                    depthAttachment.ArrayLayer,
                    depthAttachment.MipLevel);
            }
            FramebufferAttachment[] colorTargets = new FramebufferAttachment[colorTargetDescs.Count];
            for (int i = 0; i < colorTargets.Length; i++)
            {
                colorTargets[i] = new FramebufferAttachment(
                    colorTargetDescs[i].Target,
                    colorTargetDescs[i].ArrayLayer,
                    colorTargetDescs[i].MipLevel);
            }

            ColorTargets = colorTargets;

            Texture dimTex;
            uint mipLevel;
            if (ColorTargets.Count > 0)
            {
                dimTex = ColorTargets[0].Target;
                mipLevel = ColorTargets[0].MipLevel;
            }
            else
            {
                Debug.Assert(DepthTarget != null);
                dimTex = DepthTarget.Value.Target;
                mipLevel = DepthTarget.Value.MipLevel;
            }

            Util.GetMipDimensions(dimTex, mipLevel, out uint mipWidth, out uint mipHeight, out _);
            Width = mipWidth;
            Height = mipHeight;


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
