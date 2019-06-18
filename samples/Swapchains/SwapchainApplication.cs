using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace Swapchains
{
    public class SwapchainApplication
    {
        private readonly List<ExtraWindow> _extraWindows = new List<ExtraWindow>();
        private readonly Sdl2Window _window;
        private readonly GraphicsDevice _gd;
        private readonly DisposeCollectorResourceFactory _rf;
        private readonly Swapchain _sc;
        private CommandList _cl;
        private ImGuiRenderer _imguiRenderer;
        private int _id;
        private float _deltaTime;
        private Random _random = new Random();

        public SwapchainApplication()
        {
            _window = VeldridStartup.CreateWindow(new WindowCreateInfo(
                Sdl2Native.SDL_WINDOWPOS_CENTERED, Sdl2Native.SDL_WINDOWPOS_CENTERED,
                960, 540,
                WindowState.Normal,
                "Swapchains Sample"));
            GraphicsDeviceOptions options = new GraphicsDeviceOptions()
            {
                ResourceBindingModel = ResourceBindingModel.Improved,
                SwapchainSrgbFormat = true,
                PreferDepthRangeZeroToOne = true,
                PreferStandardClipSpaceYDirection = true
            };
            _gd = VeldridStartup.CreateGraphicsDevice(_window, options);
            _rf = new DisposeCollectorResourceFactory(_gd.ResourceFactory);
            _sc = _rf.CreateSwapchain(new SwapchainDescription(
                VeldridStartup.GetSwapchainSource(_window), (uint)_window.Width, (uint)_window.Height, null, true, true));
            _window.Resized += () =>
            {
                _sc.Resize((uint)_window.Width, (uint)_window.Height);
                _imguiRenderer.WindowResized(_window.Width, _window.Height);
            };

            CreateResources(_gd.ResourceFactory);
        }

        public void Run()
        {
            Stopwatch sw = new Stopwatch();
            double lastFrameTime = sw.Elapsed.TotalSeconds;
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                ProcessInputs(snapshot);
                double newFrameTime = sw.Elapsed.TotalSeconds;
                float delta = (float)(newFrameTime - lastFrameTime);
                lastFrameTime = newFrameTime;
                Draw(delta);
            }

            _rf.DisposeCollector.DisposeAll();
            _gd.Dispose();
        }

        private void CreateResources(ResourceFactory factory)
        {
            _cl = factory.CreateCommandList();
            _imguiRenderer = new ImGuiRenderer(
                _gd,
                _gd.MainSwapchain.Framebuffer.OutputDescription,
                (int)_window.Width, (int)_window.Height, ColorSpaceHandling.Linear);
        }

        private void ProcessInputs(InputSnapshot snapshot) => _imguiRenderer.Update(_deltaTime, snapshot);

        private void Draw(float deltaSeconds)
        {
            _deltaTime = deltaSeconds;
            DrawGui();

            _cl.Begin();
            ProcessOtherWindows();
            _cl.SetFramebuffer(_sc.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(0f, 0f, 0.2f, 1f));
            _imguiRenderer.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_sc);

            foreach (ExtraWindow window in _extraWindows) { _gd.SwapBuffers(window.Swapchain); }
        }

        private void DrawGui()
        {
            if (ImGui.Button("Create new window"))
            {
                RgbaFloat randomColor = new RgbaFloat(
                    (float)_random.NextDouble(),
                    (float)_random.NextDouble(),
                    (float)_random.NextDouble(),
                    1);
                _extraWindows.Add(new ExtraWindow(_gd, _rf, $"Window{_id++}", randomColor));
            }
            ImGui.Separator();

            foreach (ExtraWindow window in _extraWindows)
            {
                DrawWindowControls(window);
            }
        }

        private void DrawWindowControls(ExtraWindow window)
        {
            ImGui.PushID(window.GetHashCode());
            Vector3 color3 = new Vector3(window.Color.R, window.Color.G, window.Color.B);
            if (ImGui.ColorEdit3($"Clear Color", ref color3))
            {
                window.Color = new RgbaFloat(color3.X, color3.Y, color3.Z, 1);
            }
            if (ImGui.Button("Close"))
            {
                window.Close();
            }

            ImGui.Separator();
            ImGui.PopID();
        }

        private void ProcessOtherWindows()
        {
            for (int i = 0; i < _extraWindows.Count; i++)
            {
                ExtraWindow window = _extraWindows[i];
                if (!window.Exists)
                {
                    _extraWindows.RemoveAt(i);
                    i -= 1;
                }

                window.ProcessEvents();
                _cl.SetFramebuffer(window.Swapchain.Framebuffer);
                _cl.ClearColorTarget(0, window.Color);
            }
        }
    }

    public class ExtraWindow
    {
        private readonly Sdl2Window _window;
        public Swapchain Swapchain { get; }
        public string Name { get; }
        public RgbaFloat Color { get; set; }
        public bool Exists => _window.Exists;

        public ExtraWindow(GraphicsDevice gd, ResourceFactory rf, string name, RgbaFloat color)
        {
            _window = VeldridStartup.CreateWindow(new WindowCreateInfo(400, 400, 300, 175, WindowState.Normal, name));
            SwapchainSource ss = VeldridStartup.GetSwapchainSource(_window);
            Swapchain = rf.CreateSwapchain(new SwapchainDescription(
                ss, (uint)_window.Width, (uint)_window.Height, null, false));
            Name = name;
            Color = color;
        }

        public void ProcessEvents()
        {
            InputSnapshot snapshot = _window.PumpEvents();
        }

        internal void Close()
        {
            _window.Close();
        }
    }
}
