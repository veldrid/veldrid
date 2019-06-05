using System;
using System.Runtime.CompilerServices;
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
        private DeviceBuffer _mainFBInfoBuffer;

        public void Initialize(IGalleryDriver gallery)
        {
            Gallery = gallery;
            Device = gallery.Device;
            _factory = new DisposeCollectorResourceFactory(Device.ResourceFactory);
            RecreateFramebuffer();
            gallery.Resized += OnGallerySizeChanged;

            GalleryConfig.Global.MainFBInfoLayout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("FBInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));
            _mainFBInfoBuffer = Device.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)Unsafe.SizeOf<FBInfo>(), BufferUsage.UniformBuffer));
            Device.UpdateBuffer(
                _mainFBInfoBuffer,
                0,
                new FBInfo() { Width = Framebuffer.Width, Height = Framebuffer.Height });
            GalleryConfig.Global.MainFBInfoSet = Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                GalleryConfig.Global.MainFBInfoLayout, _mainFBInfoBuffer));
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
            GalleryConfig.Global.MainFB = Framebuffer;

            if (_mainFBInfoBuffer != null)
            {
                Device.UpdateBuffer(
                    _mainFBInfoBuffer,
                    0,
                    new FBInfo() { Width = Framebuffer.Width, Height = Framebuffer.Height });
            }

        }

        public void Shutdown()
        {
            _factory.DisposeCollector.DisposeAll();
        }

        public abstract Task LoadResourcesAsync();
        public abstract void Render(double deltaSeconds);
    }
}
