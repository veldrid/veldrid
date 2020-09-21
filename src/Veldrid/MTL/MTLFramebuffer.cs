namespace Veldrid.MTL
{
    internal class MTLFramebuffer : MTLFramebufferBase
    {
        public override bool IsRenderable => true;
        private bool _disposed;

        public override bool IsDisposed => _disposed;

        public MTLFramebuffer(MTLGraphicsDevice gd, ref FramebufferDescription description)
            : base(gd, ref description)
        {
        }

        protected override MetalBindings.MTLTexture GetMtlTexture(uint target)
        {
            return Util.AssertSubtype<Texture, MTLTexture>(ColorTargets[(int)target].Target).DeviceTexture;
        }

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
