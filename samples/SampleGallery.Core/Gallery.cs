using ImGuiNET;
using System.Runtime.CompilerServices;

namespace Veldrid.SampleGallery
{
    public class Gallery
    {
        private IGalleryDriver _driver;
        private GraphicsDevice _gd;
        private Swapchain _mainSwapchain;
        private ImGuiRenderer _imguiRenderer;
        private CommandList _cl;
        private Example _example;

        public Gallery(IGalleryDriver driver)
        {
            _driver = driver;
            _gd = driver.Device;
            _mainSwapchain = _driver.MainSwapchain;

            _driver.Resized += () =>
            {
                _mainSwapchain.Resize(_driver.Width, _driver.Height);
                _imguiRenderer.WindowResized((int)_driver.Width, (int)_driver.Height);
            };

            _driver.Update += Update;
            _driver.Render += Render;

            _imguiRenderer = new ImGuiRenderer(
                _gd,
                _mainSwapchain.Framebuffer.OutputDescription,
                (int)_driver.Width, (int)_driver.Height,
                ColorSpaceHandling.Linear);
            _cl = _gd.ResourceFactory.CreateCommandList();
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
        }

        private void Render(double deltaSeconds)
        {
            _example?.Render(deltaSeconds); // _example.Framebuffer now contains output.

            ImGui.Text($"Framerate: {ImGui.GetIO().Framerate}");
            ImGui.Text($"Mouse pos: {ImGui.GetIO().MousePos}");
            ImGui.Text($"Backend: {_driver.Device.BackendType}");

            _cl.Begin();
            _cl.SetFramebuffer(_mainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(0, 0, 0.2f, 1));
            _cl.ClearDepthStencil(1f);
            if (_example != null)
            {
                _cl.BlitTexture(
                    _example.Framebuffer.ColorTargets[0].Target, 0, 0, _example.Framebuffer.Width, _example.Framebuffer.Height,
                    _mainSwapchain.Framebuffer, 0, 0, _mainSwapchain.Framebuffer.Width, _mainSwapchain.Framebuffer.Height,
                    false);
            }

            _imguiRenderer.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_mainSwapchain);
            _gd.WaitForIdle();
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
