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
        protected FramebufferAttachment[] _colorTargets = Array.Empty<FramebufferAttachment>();

        /// <summary>
        /// Gets the depth attachment associated with this instance. May be null if no depth texture is used.
        /// </summary>
        public FramebufferAttachment? DepthTarget => _depthTarget;

        /// <summary>
        /// Gets the collection of color attachments associated with this instance. May be empty.
        /// </summary>
        public ReadOnlySpan<FramebufferAttachment> ColorTargets => _colorTargets;

        /// <summary>
        /// Gets an <see cref="Veldrid.OutputDescription"/> which describes the number and formats of the depth and color targets
        /// in this instance.
        /// </summary>
        public OutputDescription OutputDescription { get; protected set; }

        /// <summary>
        /// Gets the width of the <see cref="Framebuffer"/>.
        /// </summary>
        public uint Width { get; protected set; }

        /// <summary>
        /// Gets the height of the <see cref="Framebuffer"/>.
        /// </summary>
        public uint Height { get; protected set; }

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

            Texture dimTex;
            uint mipLevel;
            if (colorTargets.Length > 0)
            {
                dimTex = colorTargets[0].Target;
                mipLevel = colorTargets[0].MipLevel;
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

            _colorTargets = colorTargets;
            OutputDescription = OutputDescription.CreateFromFramebuffer(this);
        }

        /// <inheritdoc/>
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
