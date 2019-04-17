using System;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace Veldrid.SampleGallery
{
    public abstract class Example
    {
        protected GraphicsDevice Device { get; private set; }
        protected IGalleryDriver Gallery { get; private set; }
        protected ResourceFactory Factory => _factory;
        public Framebuffer Framebuffer { get; private set; }
        private DisposeCollectorResourceFactory _factory;

        public void Initialize(IGalleryDriver gallery)
        {
            Gallery = gallery;
            Device = gallery.Device;
            _factory = new DisposeCollectorResourceFactory(Device.ResourceFactory);
            RecreateFramebuffer();
            gallery.Resized += OnGallerySizeChanged;
        }

        private void OnGallerySizeChanged()
        {
            RecreateFramebuffer();
            OnGallerySizeChangedCore();
        }

        protected virtual void OnGallerySizeChangedCore() { }

        private void RecreateFramebuffer()
        {
            Framebuffer?.Dispose();

            Texture color = Factory.CreateTexture(
                TextureDescription.Texture2D(
                    Gallery.Width, Gallery.Height, 1, 1,
                    PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                    TextureUsage.Sampled | TextureUsage.RenderTarget));
            Texture depth = Factory.CreateTexture(
                TextureDescription.Texture2D(
                    Gallery.Width, Gallery.Height, 1, 1,
                    PixelFormat.R16_UNorm,
                    TextureUsage.DepthStencil));
            Framebuffer = Factory.CreateFramebuffer(new FramebufferDescription(depth, color));
        }

        public void Shutdown()
        {
            _factory.DisposeCollector.DisposeAll();
        }

        public abstract Task LoadResourcesAsync();
        public abstract void Render(double deltaSeconds);
    }
}
