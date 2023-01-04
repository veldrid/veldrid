using System;
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
        protected IGalleryDriver Driver { get; private set; }
        protected ResourceFactory Factory => _factory;
        protected uint FrameIndex => Driver.FrameIndex;
        public Framebuffer[] Framebuffers { get; private set; }
        public ResourceSet[] BlitterSets => _blitterSets;

        public void Initialize(IGalleryDriver gallery)
        {
            Driver = gallery;
            Device = gallery.Device;
            _factory = new DisposeCollectorResourceFactory(Device.ResourceFactory);
            _mainFBInfoBuffer = Device.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)Unsafe.SizeOf<FBInfo>(), BufferUsage.UniformBuffer));
            Console.WriteLine("Calling RecreateFramebuffer.");
            RecreateFramebuffer();
            Console.WriteLine("Finished calling RecreateFramebuffer.");
            gallery.Resized += OnGallerySizeChanged;

            Console.WriteLine("Creating MainFBInfoLayout.");
            GalleryConfig.Global.MainFBInfoLayout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("FBInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));
            Console.WriteLine("Creating MainFBInfoSet.");
            GalleryConfig.Global.MainFBInfoSet = Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                GalleryConfig.Global.MainFBInfoLayout, _mainFBInfoBuffer));

            PostInitialize();
        }

        protected virtual void PostInitialize()
        {
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
            Framebuffers = new Framebuffer[Driver.BufferCount];

            Console.WriteLine($"Entering Framebuffers[] creation loop.");
            for (uint i = 0; i < Driver.BufferCount; i++)
            {
                Texture color = Factory.CreateTexture(
                    TextureDescription.Texture2D(
                        Driver.Width, Driver.Height, 1, 1,
                        PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                        TextureUsage.Sampled | TextureUsage.RenderTarget));
                Texture depth = Factory.CreateTexture(
                    TextureDescription.Texture2D(
                        Driver.Width, Driver.Height, 1, 1,
                        PixelFormat.R16_UNorm,
                        TextureUsage.DepthStencil));
                Framebuffers[i] = Factory.CreateFramebuffer(new FramebufferDescription(depth, color));
            }

            Console.WriteLine($"Finished Framebuffers[] creation loop.");

            GalleryConfig.Global.MainFBOutput = Framebuffers[0].OutputDescription;
            GalleryConfig.Global.ViewWidth = Driver.Width;
            GalleryConfig.Global.ViewHeight = Driver.Height;

            if (_mainFBInfoBuffer != null)
            {
                Device.UpdateBuffer(
                    _mainFBInfoBuffer,
                    0,
                    new FBInfo() { Width = Framebuffers[0].Width, Height = Framebuffers[0].Height });
            }

            Util.DisposeAll(_blitterSets);
            _blitterSets = new ResourceSet[Driver.BufferCount];
            for (uint i = 0; i < Driver.BufferCount; i++)
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
        public abstract CommandBuffer[] Render(double deltaSeconds);

        public virtual void DrawMainMenuBars()
        {
        }
    }

    /// <summary>
    /// A simple example base class which renders into a single CommandBuffer which is re-recorded every frame.
    /// </summary>
    public abstract class BasicExample : Example
    {
        CommandBuffer[][] _frameCBs;

        protected override void PostInitialize()
        {
            _frameCBs = new CommandBuffer[Driver.BufferCount][];
            for (int i = 0; i < _frameCBs.Length; i++)
            {
                _frameCBs[i] = new[] { Factory.CreateCommandBuffer() };
            }
        }

        public override CommandBuffer[] Render(double deltaSeconds)
        {
            Render(deltaSeconds, _frameCBs[Driver.FrameIndex][0]);
            return _frameCBs[Driver.FrameIndex];
        }

        protected abstract void Render(double deltaSeconds, CommandBuffer commandBuffer);
    }
}
