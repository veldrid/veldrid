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
        /// Constructs a new FramebufferAttachment.
        /// </summary>
        /// <param name="target">The target <see cref="Texture"/> which will be rendered to.</param>
        /// <param name="arrayLayer">The target array layer.</param>
        public FramebufferAttachment(Texture target, uint arrayLayer)
        {
            Target = target;
            ArrayLayer = arrayLayer;
        }
    }
}
