namespace Veldrid.MTL
{
    // A fake Texture object representing swapchain Textures.
    internal sealed class MTLPlaceholderTexture : Texture
    {
        private bool _disposed;

        public override string? Name { get; set; }

        public override bool IsDisposed => _disposed;

        public MTLPlaceholderTexture(PixelFormat format)
        {
            Format = format;
            Depth = 1;
            MipLevels = 1;
            ArrayLayers = 1;
            Usage = TextureUsage.RenderTarget;
            Type = TextureType.Texture2D;
            SampleCount = TextureSampleCount.Count1;
        }

        public void Resize(uint width, uint height)
        {
            Width = width;
            Height = height;
        }

        private protected override void DisposeCore()
        {
            _disposed = true;
        }
    }
}
