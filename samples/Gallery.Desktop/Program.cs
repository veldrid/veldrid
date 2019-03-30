using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using System.Diagnostics;

namespace Veldrid.SampleGallery
{
    class Program
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;

        static void Main(string[] args)
        {
#if DEBUG
            bool isDebugBuild = true;
#else
            bool isDebugBuild = false;
#endif
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: isDebugBuild,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: true);
            GraphicsBackend backend = VeldridStartup.GetPlatformDefaultBackend();
            WindowCreateInfo windowCI = new WindowCreateInfo(
                Sdl2Native.SDL_WINDOWPOS_CENTERED, Sdl2Native.SDL_WINDOWPOS_CENTERED,
                1280, 720,
                WindowState.Normal,
                "Veldrid Sample Gallery");
            VeldridStartup.CreateWindowAndGraphicsDevice(windowCI, options, backend, out _window, out _gd);

            Example example = new SimpleMeshRender();
            example.Initialize(_gd, _gd.MainSwapchain);
            example.LoadResourcesAsync().Wait();

            Stopwatch sw = Stopwatch.StartNew();
            double previousTime = sw.Elapsed.TotalSeconds;

            while (_window.Exists)
            {
                double currentTime = sw.Elapsed.TotalSeconds;
                double elapsed = currentTime - previousTime;
                currentTime = previousTime;

                InputSnapshot snapshot = _window.PumpEvents();

                example.Render(elapsed);
            }
        }
    }
}
