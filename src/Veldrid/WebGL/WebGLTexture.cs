namespace Veldrid.WebGL
{
    internal class WebGLTexture : Texture
    {
        private readonly WebGLGraphicsDevice _gd;

        public uint Target { get; }
        public WebGLDotNET.WebGLTexture WglTexture { get; }

        public WebGLTexture(WebGLGraphicsDevice gd, ref TextureDescription description)
        {
            _gd = gd;
        }

        public override PixelFormat Format => throw new System.NotImplementedException();

        public override uint Width => throw new System.NotImplementedException();

        public override uint Height => throw new System.NotImplementedException();

        public override uint Depth => throw new System.NotImplementedException();

        public override uint MipLevels => throw new System.NotImplementedException();

        public override uint ArrayLayers => throw new System.NotImplementedException();

        public override TextureUsage Usage => throw new System.NotImplementedException();

        public override TextureType Type => throw new System.NotImplementedException();

        public override TextureSampleCount SampleCount => throw new System.NotImplementedException();

        public override string Name { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        private protected override void DisposeCore()
        {
            throw new System.NotImplementedException();
        }
    }
}
