using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace Veldrid.SampleGallery
{
    public abstract class Example
    {
        private DisposeCollectorResourceFactory _factory;
        private DeviceBuffer _mainFBInfoBuffer;
        private ResourceSet[] _blitterSets;

        protected GraphicsDevice Device { get; private set; }
        protected IGalleryDriver Gallery { get; private set; }
        protected ResourceFactory Factory => _factory;
        protected uint FrameIndex => Gallery.FrameIndex;
        public Framebuffer[] Framebuffers { get; private set; }
        public ResourceSet[] BlitterSets => _blitterSets;

        public void Initialize(IGalleryDriver gallery)
        {
            Gallery = gallery;
            Device = gallery.Device;
            _factory = new DisposeCollectorResourceFactory(Device.ResourceFactory);
            _mainFBInfoBuffer = Device.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)Unsafe.SizeOf<FBInfo>(), BufferUsage.UniformBuffer));
            RecreateFramebuffer();
            gallery.Resized += OnGallerySizeChanged;

            GalleryConfig.Global.MainFBInfoLayout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("FBInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));
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
            Util.DisposeAll(Framebuffers);
            Framebuffers = new Framebuffer[Gallery.BufferCount];

            for (uint i = 0; i < Gallery.BufferCount; i++)
            {
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
                Framebuffers[i] = Factory.CreateFramebuffer(new FramebufferDescription(depth, color));
            }

            GalleryConfig.Global.MainFBOutput = Framebuffers[0].OutputDescription;
            GalleryConfig.Global.ViewWidth = Gallery.Width;
            GalleryConfig.Global.ViewHeight = Gallery.Height;

            if (_mainFBInfoBuffer != null)
            {
                Device.UpdateBuffer(
                    _mainFBInfoBuffer,
                    0,
                    new FBInfo() { Width = Framebuffers[0].Width, Height = Framebuffers[0].Height });
            }

            Util.DisposeAll(_blitterSets);
            _blitterSets = new ResourceSet[Gallery.BufferCount];
            for (uint i = 0; i < Gallery.BufferCount; i++)
            {
                _blitterSets[i] = Factory.CreateResourceSet(new ResourceSetDescription(
                    GalleryConfig.Global.BlitterLayout,
                    Framebuffers[i].ColorTargets[0].Target,
                    Device.PointSampler));
            }
        }

        public void Shutdown()
        {
            _factory.DisposeCollector.DisposeAll();
        }

        public abstract Task LoadResourcesAsync();
        public abstract void Render(double deltaSeconds, CommandBuffer cb);
    }
}
