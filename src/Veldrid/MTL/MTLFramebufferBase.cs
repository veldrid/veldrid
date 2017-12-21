using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal abstract class MTLFramebufferBase : Framebuffer
    {
        public abstract MTLRenderPassDescriptor CreateRenderPassDescriptor();
        public abstract bool IsRenderable { get; }

        public override string Name
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public MTLFramebufferBase(MTLGraphicsDevice gd, ref FramebufferDescription description)
            : base(description.DepthTarget, description.ColorTargets)
        {
        }

        public MTLFramebufferBase()
        {
        }
    }
}