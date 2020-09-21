namespace Veldrid.OpenGL
{
    internal class OpenGLPlaceholderTexture : Texture
    {
        private uint _height;
        private uint _width;
        private bool _disposed;

        public OpenGLPlaceholderTexture(
            uint width,
            uint height,
            PixelFormat format,
            TextureUsage usage,
            TextureSampleCount sampleCount)
        {
            _width = width;
            _height = height;
            Format = format;
            Usage = usage;
            SampleCount = sampleCount;
        }

        public void Resize(uint width, uint height)
        {
            _width = width;
            _height = height;
        }

        public override PixelFormat Format { get; }

        public override uint Width => _width;

        public override uint Height => _height;

        public override uint Depth => 1;

        public override uint MipLevels => 1;

        public override uint ArrayLayers => 1;

        public override TextureUsage Usage { get; }

        public override TextureSampleCount SampleCount { get; }

        public override TextureType Type => TextureType.Texture2D;

        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        private protected override void DisposeCore()
        {
            _disposed = true;
        }
    }
}
