namespace Veldrid
{
    /// <summary>
    /// Represents a single output of a <see cref="Framebuffer"/>. May be a color or depth attachment.
    /// </summary>
    public struct FramebufferAttachment
    {
        /// <summary>
        /// The target <see cref="Texture"/> which will be rendered to.
        /// </summary>
        public Texture Target { get; }
        /// <summary>
        /// The target array layer.
        /// </summary>
        public uint ArrayLayer { get; }
        /// <summary>
        /// The target mip level.
        /// </summary>
        public uint MipLevel { get; }
        /// <summary>
        /// Indicates whether the target should be bound as a layered target. Used for texture arrays and cubemaps.
        /// </summary>
        public bool LayeredTarget { get; }

        /// <summary>
        /// Constructs a new FramebufferAttachment.
        /// </summary>
        /// <param name="target">The target <see cref="Texture"/> which will be rendered to.</param>
        /// <param name="arrayLayer">The target array layer.</param>
        public FramebufferAttachment(Texture target, uint arrayLayer)
        {
            Target = target;
            ArrayLayer = arrayLayer;
            MipLevel = 0;
            LayeredTarget = false;
        }

        /// <summary>
        /// Constructs a new FramebufferAttachment.
        /// </summary>
        /// <param name="target">The target <see cref="Texture"/> which will be rendered to.</param>
        /// <param name="arrayLayer">The target array layer.</param>
        /// <param name="mipLevel">The target mip level.</param>
        public FramebufferAttachment(Texture target, uint arrayLayer, uint mipLevel)
        {
            Target = target;
            ArrayLayer = arrayLayer;
            MipLevel = mipLevel;
            LayeredTarget = false;
        }

        /// <summary>
        /// Constructs a new FramebufferAttachment.
        /// </summary>
        /// <param name="target">The target <see cref="Texture"/> which will be rendered to.</param>
        /// <param name="arrayLayer">The target array layer.</param>
        /// <param name="mipLevel">The target mip level.</param>
        /// <param name="layeredTarget">Whether to bind the target as a layered target.</param>
        public FramebufferAttachment(Texture target, uint arrayLayer, uint mipLevel, bool layeredTarget)
        {
            Target = target;
            ArrayLayer = arrayLayer;
            MipLevel = mipLevel;
            LayeredTarget = layeredTarget;
        }
    }
}
