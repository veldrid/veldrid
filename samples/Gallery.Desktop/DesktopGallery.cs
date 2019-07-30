using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using System.Diagnostics;
using System;
using Snake;

namespace Veldrid.SampleGallery
{
    public class DesktopGallery : IGalleryDriver
    {
        private Sdl2Window _window;
        private GraphicsDevice _gd;
        private readonly InputState _inputState = new InputState();
        private AdvancedFrameLoop _frameLoop;
        private bool _needsResize;
        private Stopwatch _sw;
        private double _previousTime;

        public uint Width => (uint)_window.Width;

        public uint Height => (uint)_window.Height;

        public event Action Resized;
        public event Action<double> Update;
        public event Func<double, CommandBuffer[]> Render;

        public GraphicsDevice Device => _gd;

        public Swapchain MainSwapchain => _gd.MainSwapchain;

        public uint FrameIndex => _frameLoop.FrameIndex;
        public uint BufferCount => MainSwapchain.BufferCount;

        public bool SupportsImGui => true;

        public void Run(string[] args)
        {
            GraphicsDeviceOptions options = Gallery.GetPreferredOptions();
            GraphicsBackend backend = VeldridStartup.GetPlatformDefaultBackend();

            WindowCreateInfo windowCI = new WindowCreateInfo(
                Sdl2Native.SDL_WINDOWPOS_CENTERED, Sdl2Native.SDL_WINDOWPOS_CENTERED,
                1280, 720,
                WindowState.Normal,
                "Veldrid Sample Gallery");

            VeldridStartup.CreateWindowAndGraphicsDevice(windowCI, options, backend, out _window, out _gd);
            _frameLoop = new AdvancedFrameLoop(_gd, _gd.MainSwapchain);
            _window.Resized += () =>
            {
                _needsResize = true;
            };

            Gallery gallery = new Gallery(this);
            gallery.RegisterExample("Simple Mesh Render", () => new SimpleMeshRender());
            gallery.RegisterExample("Snake", () => new SnakeExample());
            gallery.LoadExample("Simple Mesh Render");

            _sw = Stopwatch.StartNew();
            _previousTime = _sw.Elapsed.TotalSeconds;
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                _inputState.Clear();
                _inputState.AddSnapshot(snapshot);
                _inputState.MouseDelta = _window.MouseDelta;
                if (!_window.Exists) { break; }

                if (_needsResize)
                {
                    _needsResize = false;
                    MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                    Resized?.Invoke();
                }

                _frameLoop.RunFrame(FrameHandler);
            }
        }

        private CommandBuffer[] FrameHandler(uint frameIndex, Framebuffer fb)
        {
            double currentTime = _sw.Elapsed.TotalSeconds;
            double elapsed = currentTime - _previousTime;
            _previousTime = currentTime;

            Update?.Invoke(elapsed);
            return Render?.Invoke(elapsed);
        }

        public InputStateView GetInputState() => _inputState.View;
    }
}
