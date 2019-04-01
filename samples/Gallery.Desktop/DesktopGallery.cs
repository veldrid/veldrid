using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using System.Diagnostics;
using System;

namespace Veldrid.SampleGallery
{
    public class DesktopGallery : IGalleryDriver
    {
        private Sdl2Window _window;
        private GraphicsDevice _gd;

        public uint Width => (uint)_window.Width;

        public uint Height => (uint)_window.Height;

        public event Action Resized;
        public event Action<double, InputSnapshot> Update;
        public event Action<double> Render;

        public GraphicsDevice Device => _gd;

        public Swapchain MainSwapchain => _gd.MainSwapchain;

        public void Run(string[] args)
        {
            GraphicsDeviceOptions options = Gallery.GetPreferredOptions();
            GraphicsBackend backend = GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan)
                ? GraphicsBackend.Vulkan
                : VeldridStartup.GetPlatformDefaultBackend();
            WindowCreateInfo windowCI = new WindowCreateInfo(
                Sdl2Native.SDL_WINDOWPOS_CENTERED, Sdl2Native.SDL_WINDOWPOS_CENTERED,
                1280, 720,
                WindowState.Normal,
                "Veldrid Sample Gallery");
            VeldridStartup.CreateWindowAndGraphicsDevice(windowCI, options, backend, out _window, out _gd);
            _window.Resized += () =>
            {
                Resized?.Invoke();
            };

            Gallery gallery = new Gallery(this);
            gallery.LoadExample(new SimpleMeshRender());

            Stopwatch sw = Stopwatch.StartNew();
            double previousTime = sw.Elapsed.TotalSeconds;
            while (_window.Exists)
            {
                double currentTime = sw.Elapsed.TotalSeconds;
                double elapsed = currentTime - previousTime;
                previousTime = currentTime;

                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }

                Update?.Invoke(elapsed, snapshot);
                Render?.Invoke(elapsed);
            }
        }
    }
}
