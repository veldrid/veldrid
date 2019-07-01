using ImGuiNET;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Veldrid.SampleGallery
{
    public class Gallery
    {
        private IGalleryDriver _driver;
        private GraphicsDevice _gd;
        private Swapchain _mainSwapchain;
        private ImGuiCBRenderer _imguiRenderer;
        private Example _example;
        private TextureBlitter _blitter;

        public Gallery(IGalleryDriver driver)
        {
            _driver = driver;
            _gd = driver.Device;
            _mainSwapchain = _driver.MainSwapchain;

            _driver.Resized += () =>
            {
                _imguiRenderer.WindowResized((int)_driver.Width, (int)_driver.Height);
            };

            _driver.Update += Update;
            _driver.Render += Render;

            _imguiRenderer = new ImGuiCBRenderer(
                _gd,
                _mainSwapchain.Framebuffer.OutputDescription,
                (int)_driver.Width, (int)_driver.Height,
                ColorSpaceHandling.Linear,
                _mainSwapchain.BufferCount);
            _blitter = new TextureBlitter(_gd, _gd.ResourceFactory, _mainSwapchain.Framebuffer.OutputDescription, false);
            GalleryConfig.Global.BlitterLayout = _blitter.ResourceLayout;
        }

        public void LoadExample(Example example)
        {
            _example = example;
            _example.Initialize(_driver);
            _example.LoadResourcesAsync().Wait();
        }

        private void Update(double deltaSeconds)
        {
            InputTracker.UpdateFrameInput(_driver.GetInputState());
            _imguiRenderer.Update((float)deltaSeconds, _driver.GetInputState());

            ImGui.Text($"Framerate: {ImGui.GetIO().Framerate}");
            ImGui.Text($"Mouse pos: {ImGui.GetIO().MousePos}");
            ImGui.Text($"Backend: {_driver.Device.BackendType}");
        }

        private void Render(double deltaSeconds, CommandBuffer cb)
        {
            _example?.Render(deltaSeconds, cb); // _example.Framebuffer now contains output.

            cb.MemoryBarrier(
                _example.Framebuffers[_driver.FrameIndex].ColorTargets[0].Target, 0, 1, 0, 1,
                ShaderStages.Fragment,
                ShaderStages.Fragment);

            cb.BeginRenderPass(
                _mainSwapchain.Framebuffers[_driver.FrameIndex],
                LoadAction.Clear,
                StoreAction.Store,
                default,
                1f);
            _blitter.Render(cb, _example.BlitterSets[_driver.FrameIndex], Vector2.Zero, Vector2.One);
            _imguiRenderer.Render(_gd, cb);
            cb.EndRenderPass();
        }

        public static GraphicsDeviceOptions GetPreferredOptions()
        {
#if DEBUG
            bool isDebugBuild = true;
#else
            bool isDebugBuild = false;
#endif

            return new GraphicsDeviceOptions(
                debug: isDebugBuild,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: true);
        }
    }
}
