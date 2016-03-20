using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Platform;

namespace Veldrid.RenderDemo
{
    public static class Program
    {
        private static RenderContext _rc;
        private static FrameTimeAverager _fta;
        private static double _desiredFrameLengthMilliseconds = 1000.0 / 60.0;
        private static OpenTKWindow _window;
        private static bool _limitFrameRate = false;
        private static FlatListVisibilityManager _visiblityManager;
        private static ConstantDataProvider<DirectionalLightBuffer> _lightBufferProvider;
        private static DynamicDataProvider<Matrix4x4> _viewMatrixProvider = new DynamicDataProvider<Matrix4x4>();

        private static ImageProcessorTexture _altBufferImage;
        private static bool _takeScreenshot;
        private static ImGuiRenderer _imguiRenderer;

        public static void Main()
        {
            try
            {
                _window = new DedicatedThreadWindow();
                _rc = new D3DRenderContext(_window);

                _imguiRenderer = new ImGuiRenderer(_rc, _window.NativeWindow);
                _altBufferImage = new ImageProcessorTexture(new ImageProcessor.Image(_window.Width, _window.Height));
                _lightBufferProvider = new ConstantDataProvider<DirectionalLightBuffer>(
                    new DirectionalLightBuffer(RgbaFloat.White, new Vector3(-.3f, -1f, -1f)));
                _rc.DataProviders.Add("LightBuffer", _lightBufferProvider);
                _rc.DataProviders.Add("ViewMatrix", _viewMatrixProvider);
                _rc.ClearColor = RgbaFloat.CornflowerBlue;

                CreateScreenshotFramebuffer();
                CreateWireframeRasterizerState();

                _visiblityManager = SceneWithTeapot();

                _fta = new FrameTimeAverager(666);

                long previousFrameTicks = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (_rc.Window.Exists)
                {
                    long currentFrameTicks = sw.ElapsedTicks;
                    double deltaMilliseconds = (currentFrameTicks - previousFrameTicks) * (1000.0 / Stopwatch.Frequency);

                    while (_limitFrameRate && deltaMilliseconds < _desiredFrameLengthMilliseconds)
                    {
                        Thread.Sleep(0);
                        currentFrameTicks = sw.ElapsedTicks;
                        deltaMilliseconds = (currentFrameTicks - previousFrameTicks) * (1000.0 / Stopwatch.Frequency);
                    }

                    previousFrameTicks = currentFrameTicks;

                    var snapshot = _window.GetInputSnapshot();
                    Update(snapshot, deltaMilliseconds);
                    Draw(snapshot);
                }

            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                Console.WriteLine("Error: " + e);
                if (_rc is OpenGLRenderContext)
                {
                    Console.WriteLine("GL Error: " + GL.GetError());
                }
            }
        }

        private static void CreateWireframeRasterizerState()
        {
            _wireframeRasterizerState = _rc.ResourceFactory.CreateRasterizerState(
                                FaceCullingMode.None, TriangleFillMode.Wireframe, true, true);
        }

        private static void CreateScreenshotFramebuffer()
        {
            _alternateFramebuffer = _rc.ResourceFactory.CreateFramebuffer(_window.Width, _window.Height);
        }

        private static FlatListVisibilityManager SceneWithBoxes()
        {
            if (_boxSceneVM == null)
            {
                _boxSceneVM = new FlatListVisibilityManager();
                var tcr = new TexturedCubeRenderer(_rc);
                tcr.Position = new Vector3(-5f, 0, -3);
                _boxSceneVM.AddRenderItem(tcr);

                for (int x = 0; x < 6; x++)
                {
                    for (int y = 0; y < 6; y++)
                    {
                        for (int z = 0; z < 6; z++)
                        {
                            var ccr = new ColoredCubeRenderer(_rc);
                            ccr.Position = new Vector3((x * 1.35f) - 3, (y * 1.35f) - 3f, (z * 1.35f) - 3);
                            _boxSceneVM.AddRenderItem(ccr);
                        }
                    }
                }
            }

            return _boxSceneVM;
        }

        private static FlatListVisibilityManager SceneWithTeapot()
        {
            if (_teapotVM == null)
            {
                _teapotVM = new FlatListVisibilityManager();
                var teapot = new TeapotRenderer(_rc);
                teapot.Position = new System.Numerics.Vector3(0, -1, 0);
                _teapotVM.AddRenderItem(teapot);
            }

            return _teapotVM;
        }

        private const double TickDuration = 1000;
        private static FlatListVisibilityManager _boxSceneVM;
        private static FlatListVisibilityManager _teapotVM;
        private static double _circleWidth = 5.0;
        private static bool _wireframe;

        private static Framebuffer _alternateFramebuffer;
        private static RasterizerState _wireframeRasterizerState;

        private static void Update(InputSnapshot snapshot, double deltaMilliseconds)
        {
            float timeFactor = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            var position = new Vector3(
                (float)(Math.Cos(timeFactor) * _circleWidth),
                3 + (float)Math.Sin(timeFactor) * 2,
                (float)(Math.Sin(timeFactor) * _circleWidth));
            _viewMatrixProvider.Data = Matrix4x4.CreateLookAt(position, Vector3.Zero, Vector3.UnitY);

            _imguiRenderer.SetPerFrameImGuiData(_rc, (float)deltaMilliseconds);
            _imguiRenderer.UpdateImGuiInput(_window);

            bool opened = false;
            float width = Math.Max(100, Math.Min(200, _window.Width * .4f));
            ImGuiNative.igSetNextWindowPos(new Vector2(20, 20), SetCondition.Always);
            if (ImGui.BeginWindow("Scenes", ref opened, new Vector2(width, 200), 0.8f, WindowFlags.NoMove | WindowFlags.NoResize))
            {
                if (ImGui.Button("Boxes"))
                {
                    _circleWidth = 12.0;
                    _visiblityManager = SceneWithBoxes();
                }
                if (ImGui.Button("Teapot"))
                {
                    _circleWidth = 5.0;
                    _visiblityManager = SceneWithTeapot();
                }
                bool changed = false;
                if (ImGui.Checkbox("Wireframe", ref changed))
                {
                    _wireframe = !_wireframe;
                    if (_wireframe)
                    {
                        _rc.SetRasterizerState(_wireframeRasterizerState);
                    }
                    else
                    {
                        _rc.SetRasterizerState(_rc.DefaultRasterizerState);
                    }
                }
                bool isD3D11 = _rc is D3DRenderContext;
                bool isOpenGL = !isD3D11;

                if (isD3D11)
                {
                    ImGui.PushStyleColor(ColorTarget.Button, RgbaFloat.Cyan.ToVector4());
                    ImGui.PushStyleColor(ColorTarget.ButtonHovered, RgbaFloat.Cyan.ToVector4());
                }
                if (ImGui.Button("D3D11"))
                {
                    ChangeRenderContext(d3d: true);
                }
                if (isD3D11)
                {
                    ImGui.PopStyleColor(2);
                }
                if (isOpenGL)
                {
                    ImGui.PushStyleColor(ColorTarget.ButtonHovered, RgbaFloat.Cyan.ToVector4());
                    ImGui.PushStyleColor(ColorTarget.Button, RgbaFloat.Cyan.ToVector4());
                }
                if (ImGui.Button("OpenGL"))
                {
                    ChangeRenderContext(d3d: false);
                }
                if (isOpenGL)
                {
                    ImGui.PopStyleColor(2);
                }

                if (ImGui.Checkbox("Limit Framerate", ref _limitFrameRate))
                {
                    var threadedWindow = _window as DedicatedThreadWindow;
                    if (threadedWindow != null)
                    {
                        threadedWindow.LimitPollRate = _limitFrameRate;
                    }
                }
            }
            ImGui.EndWindow();

            _fta.AddTime(deltaMilliseconds);

            string apiName = (_rc is OpenGLRenderContext) ? "OpenGL" : "Direct3D";
            _rc.Window.Title = $"[{apiName}] " + _fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTime.ToString("#00.00 ms");

            foreach (var ke in snapshot.KeyEvents)
            {
                if (ke.Key == OpenTK.Input.Key.Escape
                    || (ke.Key == OpenTK.Input.Key.F4 && (ke.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt))
                {
                    _window.Close();
                }
                if (ke.Key == OpenTK.Input.Key.F9 && ke.Down)
                {
                    _takeScreenshot = true;
                }
                if (ke.Key == OpenTK.Input.Key.F11 && ke.Down)
                {
                    _window.WindowState = _window.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
                }

                Console.WriteLine(ke.Key + " is " + (ke.Down ? "down." : "up."));
            }

            foreach (var me in snapshot.MouseEvents)
            {
                Console.WriteLine($"MouseButton {me.MouseButton} is {(me.Down ? "down." : "up.")}");
            }
        }

        private static void ChangeRenderContext(bool d3d)
        {
            RenderContext newContext = null;
            if (d3d)
            {
                if (!(_rc is D3DRenderContext))
                {
                    newContext = new D3DRenderContext(_window);
                }
            }
            else
            {
                if (!(_rc is OpenGLRenderContext))
                {
                    newContext = new OpenGLRenderContext(_window);
                }
            }

            if (newContext != null)
            {
                foreach (var kvp in _rc.DataProviders)
                {
                    newContext.DataProviders[kvp.Key] = kvp.Value;
                }

                if (_teapotVM != null)
                {
                    foreach (var item in _teapotVM?.RenderItems)
                    {
                        item.ChangeRenderContext(newContext);
                    }
                }
                if (_boxSceneVM != null)
                {
                    foreach (var item in _boxSceneVM.RenderItems)
                    {
                        item.ChangeRenderContext(newContext);
                    }
                }
                _imguiRenderer.ChangeRenderContext(newContext);

                ((IDisposable)_rc).Dispose();
                _rc = newContext;

                CreateScreenshotFramebuffer();
                CreateWireframeRasterizerState();
                if (_wireframe)
                {
                    _rc.SetRasterizerState(_wireframeRasterizerState);
                }
            }
        }

        private unsafe static void Draw(InputSnapshot input)
        {
            _rc.ClearBuffer();

            if (_takeScreenshot)
            {
                _rc.SetFramebuffer(_alternateFramebuffer);
                _rc.ClearBuffer();
            }

            _rc.RenderFrame(_visiblityManager);

            _imguiRenderer.Render(_rc);

            if (_takeScreenshot)
            {
                _takeScreenshot = false;
                _rc.SetDefaultFramebuffer();
                _alternateFramebuffer.ColorTexture.CopyTo(_altBufferImage);
                _altBufferImage.SaveToFile(Environment.TickCount + ".png");
            }

            _rc.SwapBuffers();
        }

        private class FrameTimeAverager
        {
            private readonly double _timeLimit = 666;

            private double _accumulatedTime = 0;
            private int _frameCount = 0;
            private readonly double _decayRate = .3;

            public double CurrentAverageFrameTime { get; private set; }
            public double CurrentAverageFramesPerSecond { get { return 1000 / CurrentAverageFrameTime; } }

            public FrameTimeAverager(double maxTimeMilliseconds)
            {
                _timeLimit = maxTimeMilliseconds;
            }

            public void Reset()
            {
                _accumulatedTime = 0;
                _frameCount = 0;
            }

            public void AddTime(double frameTime)
            {
                _accumulatedTime += frameTime;
                _frameCount++;
                if (_accumulatedTime >= _timeLimit)
                {
                    Average();
                }
            }

            private void Average()
            {
                double total = _accumulatedTime;
                CurrentAverageFrameTime =
                    (CurrentAverageFrameTime * _decayRate)
                    + ((total / _frameCount) * (1 - _decayRate));

                _accumulatedTime = 0;
                _frameCount = 0;
            }
        }
    }
}