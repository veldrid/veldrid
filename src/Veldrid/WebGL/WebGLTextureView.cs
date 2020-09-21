namespace Veldrid.WebGL
{
    internal class WebGLTextureView : TextureView
    {
        private readonly WebGLGraphicsDevice _gd;
        private bool _disposed;

        public uint WglTarget { get; }
        public WebGLDotNET.WebGLTexture WglTexture { get; }
        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        public WebGLTextureView(WebGLGraphicsDevice gd, TextureViewDescription description)
            : base(ref description)
        {
            _gd = gd;
            WebGLTexture wglTargetTex = Util.AssertSubtype<Texture, WebGLTexture>(description.Target);
            WglTexture = wglTargetTex.WglTexture;
            WglTarget = wglTargetTex.Target;
        }

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
