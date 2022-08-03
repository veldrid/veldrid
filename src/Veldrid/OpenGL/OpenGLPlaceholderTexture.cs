namespace Veldrid.OpenGL
{
    internal sealed class OpenGLPlaceholderTexture : Texture
    {
        private bool _disposed;

        public OpenGLPlaceholderTexture(
            uint width,
            uint height,
            PixelFormat format,
            TextureUsage usage,
            TextureSampleCount sampleCount)
        {
            Width = width;
            Height = height;
            Format = format;
            Usage = usage;
            SampleCount = sampleCount;
            Depth = 1;
            MipLevels = 1;
            ArrayLayers = 1;
            Type = TextureType.Texture2D;
        }

        public void Resize(uint width, uint height)
        {
            Width = width;
            Height = height;
        }

        public override string? Name { get; set; }

        public override bool IsDisposed => _disposed;

        private protected override void DisposeCore()
        {
            _disposed = true;
        }
    }
}
