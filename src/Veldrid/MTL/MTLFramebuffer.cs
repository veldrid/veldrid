namespace Veldrid.MTL
{
    internal class MTLFramebuffer : MTLFramebufferBase
    {
        public override bool IsRenderable => true;

        public MTLFramebuffer(MTLGraphicsDevice gd, ref FramebufferDescription description)
            : base(gd, ref description)
        {
        }

        public override void Dispose()
        {
        }

        protected override MetalBindings.MTLTexture GetMtlTexture(uint target)
        {
            return Util.AssertSubtype<Texture, MTLTexture>(ColorTargets[(int)target].Target).DeviceTexture;
        }
    }
}