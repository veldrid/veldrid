using System;
using System.Diagnostics;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to control which color and depth textures are rendered to.
    /// See <see cref="FramebufferDescription"/>.
    /// </summary>
    public abstract class Framebuffer : DeviceResource, IDisposable
    {
        protected FramebufferAttachment? _depthTarget;
        protected FramebufferAttachment[] _colorTargets = null!;

        /// <summary>
        /// Gets the depth attachment associated with this instance. May be null if no depth texture is used.
        /// </summary>
        public virtual FramebufferAttachment? DepthTarget => _depthTarget;

        /// <summary>
        /// Gets the collection of color attachments associated with this instance. May be empty.
        /// </summary>
        public virtual ReadOnlySpan<FramebufferAttachment> ColorTargets => _colorTargets;

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

        internal Framebuffer()
        {
        }

        internal Framebuffer(
            FramebufferAttachmentDescription? depthTargetDesc,
            ReadOnlySpan<FramebufferAttachmentDescription> colorTargetDescs)
        {
            if (depthTargetDesc != null)
            {
                FramebufferAttachmentDescription depthAttachment = depthTargetDesc.Value;
                _depthTarget = new FramebufferAttachment(
                    depthAttachment.Target,
                    depthAttachment.ArrayLayer,
                    depthAttachment.MipLevel);
            }

            FramebufferAttachment[] colorTargets = new FramebufferAttachment[colorTargetDescs.Length];
            for (int i = 0; i < colorTargets.Length; i++)
            {
                colorTargets[i] = new FramebufferAttachment(
                    colorTargetDescs[i].Target,
                    colorTargetDescs[i].ArrayLayer,
                    colorTargetDescs[i].MipLevel);
            }

            _colorTargets = colorTargets;

            Texture dimTex;
            uint mipLevel;
            if (_colorTargets.Length > 0)
            {
                dimTex = _colorTargets[0].Target;
                mipLevel = _colorTargets[0].MipLevel;
            }
            else
            {
                Debug.Assert(_depthTarget != null);
                dimTex = _depthTarget.Value.Target;
                mipLevel = _depthTarget.Value.MipLevel;
            }

            Util.GetMipDimensions(dimTex, mipLevel, out uint mipWidth, out uint mipHeight);
            Width = mipWidth;
            Height = mipHeight;

            OutputDescription = OutputDescription.CreateFromFramebuffer(this);
        }

        public abstract string? Name { get; set; }

        /// <summary>
        /// A bool indicating whether this instance has been disposed.
        /// </summary>
        public abstract bool IsDisposed { get; }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
