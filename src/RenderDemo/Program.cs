using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Graphics.Pipeline;
using Veldrid.Platform;
using Veldrid.RenderDemo.ForwardRendering;
using Veldrid.RenderDemo.Models;

namespace Veldrid.RenderDemo
{
    public static class Program
    {
        private static Renderer _renderer;
        private static RenderContext _rc;
        private static FrameTimeAverager _fta;
        private static double _desiredFrameLengthMilliseconds = 1000.0 / 60.0;
        private static OpenTKWindow _window;
        private static bool _limitFrameRate = false;
        private static FlatListVisibilityManager _visiblityManager;
        private static ConstantDataProvider<DirectionalLightBuffer> _lightBufferProvider;
        private static DynamicDataProvider<Matrix4x4> _viewMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Matrix4x4> _lightViewMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Matrix4x4> _lightProjMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static FlatListVisibilityManager _boxSceneVM;
        private static FlatListVisibilityManager _teapotVM;
        private static FlatListVisibilityManager _shadowsScene;
        private static double _circleWidth = 5.0;
        private static bool _wireframe;

        private static bool _takeScreenshot;
        private static ImGuiRenderer _imguiRenderer;

        private static Framebuffer _screenshotFramebuffer;
        private static RasterizerState _wireframeRasterizerState;

        private static readonly TextureData _stoneTextureData = LoadStoneTextureData();
        private static readonly TextureData _solidWhiteTexture = LoadWhiteTextureData();

        private static Vector3 _lightPosition = new Vector3(-5f, 3f, -3f);
        private static Vector3 _lightDirection = new Vector3(5f, -3f, -3f);

        public static void Main()
        {
            try
            {
                _window = new DedicatedThreadWindow();
                _rc = new D3DRenderContext(_window);
                _renderer = new Renderer(_rc, new PipelineStage[]
                {
                    new ShadowMapStage(_rc),
                    new StandardPipelineStage("Standard"),
                    new StandardPipelineStage("Overlay"),
                });

                _imguiRenderer = new ImGuiRenderer(_rc, _window.NativeWindow);
                _lightBufferProvider = new ConstantDataProvider<DirectionalLightBuffer>(
                    new DirectionalLightBuffer(RgbaFloat.White, new Vector3(-.3f, -1f, -1f)));
                _lightViewMatrixProvider.Data = Matrix4x4.CreateLookAt(_lightPosition, Vector3.Zero, Vector3.UnitY);

                _lightProjMatrixProvider.Data = Matrix4x4.CreateOrthographicOffCenter(-10, 10, -10, 10, -25, 25);

                _rc.DataProviders.Add("LightBuffer", _lightBufferProvider);
                _rc.DataProviders.Add("ViewMatrix", _viewMatrixProvider);
                _rc.DataProviders.Add("LightViewMatrix", _lightViewMatrixProvider);
                _rc.DataProviders.Add("LightProjMatrix", _lightProjMatrixProvider);
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
            _screenshotFramebuffer = _rc.ResourceFactory.CreateFramebuffer(_window.Width, _window.Height);
        }

        private static FlatListVisibilityManager SceneWithBoxes()
        {
            if (_boxSceneVM == null)
            {
                _boxSceneVM = new FlatListVisibilityManager();
                var sphere = ObjImporter.LoadFromPath(Path.Combine(AppContext.BaseDirectory, "Models", "Sphere.obj"));
                var tcr = new TexturedMeshRenderer(_rc, sphere.Vertices, sphere.Indices);
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

                _boxSceneVM.AddRenderItem(_imguiRenderer);
            }

            return _boxSceneVM;
        }

        private static FlatListVisibilityManager SceneWithTeapot()
        {
            if (_teapotVM == null)
            {
                _teapotVM = new FlatListVisibilityManager();
                var teapot = new TeapotRenderer(_rc);
                teapot.Position = new Vector3(0, -1, 0);
                _teapotVM.AddRenderItem(teapot);
                _teapotVM.AddRenderItem(_imguiRenderer);
            }

            return _teapotVM;
        }

        private static FlatListVisibilityManager SceneWithShadows()
        {
            if (_shadowsScene == null)
            {
                _shadowsScene = new FlatListVisibilityManager();
                var sphereMeshInfo = ObjImporter.LoadFromPath(Path.Combine(AppContext.BaseDirectory, "Models", "Sphere.obj"));

                var sphere = new ShadowCaster(_rc, sphereMeshInfo.Vertices, sphereMeshInfo.Indices, _stoneTextureData);
                _shadowsScene.AddRenderItem(sphere);

                var sphere2 = new ShadowCaster(_rc, sphereMeshInfo.Vertices, sphereMeshInfo.Indices, _stoneTextureData);
                sphere2.Position = new Vector3(3f, 0f, 0f);
                _shadowsScene.AddRenderItem(sphere2);


                var plane = new ShadowCaster(_rc, PlaneModel.Vertices, PlaneModel.Indices, _solidWhiteTexture);
                plane.Position = new Vector3(0, -2.5f, 0);
                plane.Scale = new Vector3(20f);

                _shadowsScene.AddRenderItem(plane);
                _shadowsScene.AddRenderItem(_imguiRenderer);
            }

            return _shadowsScene;
        }

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
            _imguiRenderer.NewFrame();

            bool opened = false;
            float width = Math.Max(100, Math.Min(200, _window.Width * .4f));
            ImGuiNative.igSetNextWindowPos(new Vector2(20, 20), SetCondition.Always);
            if (ImGui.BeginWindow("Scenes", ref opened, new Vector2(width, 300), 0.8f, WindowFlags.NoMove | WindowFlags.NoResize))
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
                if (ImGui.Button("Shadows"))
                {
                    _circleWidth = 5.0;
                    _visiblityManager = SceneWithShadows();
                }

                if (ImGui.Checkbox("Wireframe", ref _wireframe))
                {
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

                ImGui.Text("Pipeline Stages:");
                if (ImGui.BeginChildFrame(1, new Vector2(width * .75f, 0), WindowFlags.ShowBorders))
                {
                    foreach (var stage in _renderer.Stages)
                    {
                        bool enabled = stage.Enabled;
                        if (ImGui.Checkbox(stage.Name, ref enabled))
                        {
                            stage.Enabled = !stage.Enabled;
                        }
                    }
                    ImGui.EndChildFrame();
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
                if (ke.Key == OpenTK.Input.Key.F9 && ke.Down && _window.Width * _window.Height != 0)
                {
                    _takeScreenshot = true;
                }
                if (ke.Key == OpenTK.Input.Key.F4 && ke.Down)
                {
                    ((ShadowMapStage)_renderer.Stages[0]).TakeScreenshot = true;
                }
                if (ke.Key == OpenTK.Input.Key.F11 && ke.Down)
                {
                    _window.WindowState = _window.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
                }
                if (ke.Key == OpenTK.Input.Key.Plus && ke.Down)
                {
                    _window.Width += 1;
                }
                if (ke.Key == OpenTK.Input.Key.Minus && ke.Down)
                {
                    _window.Width -= 1;
                }

                Console.WriteLine(ke.Key + " is " + (ke.Down ? "down." : "up."));
            }

            foreach (var me in snapshot.MouseEvents)
            {
                Console.WriteLine($"MouseButton {me.MouseButton} is {(me.Down ? "down." : "up.")}");
            }

            _imguiRenderer.UpdateFinished();
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
                if (_shadowsScene != null)
                {
                    foreach (var item in _shadowsScene.RenderItems)
                    {
                        item.ChangeRenderContext(newContext);
                    }
                }

                _rc.Dispose();
                _rc = newContext;

                CreateWireframeRasterizerState();
                if (_wireframe)
                {
                    _rc.SetRasterizerState(_wireframeRasterizerState);
                }

                _renderer.RenderContext = newContext;
            }
        }

        private unsafe static void Draw(InputSnapshot input)
        {
            if (_takeScreenshot)
            {
                CreateScreenshotFramebuffer();
                _rc.SetFramebuffer(_screenshotFramebuffer);
                _rc.ClearBuffer();
            }

            _renderer.RenderFrame(_visiblityManager);

            if (_takeScreenshot)
            {
                _takeScreenshot = false;
                _rc.SetDefaultFramebuffer();
                int width = _window.Width;
                int height = _window.Height;
                var cpuDepthTexture = new RawTextureDataArray<ushort>(width, height, sizeof(ushort), Graphics.PixelFormat.Alpha_UInt16);
                _screenshotFramebuffer.DepthTexture.CopyTo(cpuDepthTexture);

                ImageProcessorCore.Image image = new ImageProcessorCore.Image(width, height);
                PixelFormatConversion.ConvertPixelsUInt16DepthToRgbaFloat(width * height, cpuDepthTexture.PixelData, image.Pixels);
                ImageProcessorTexture rgbaDepthTexture = new ImageProcessorTexture(image);
                Console.WriteLine($"Saving file: {width} x {height}, ratio:{(double)width / height}");
                rgbaDepthTexture.SaveToFile(Environment.TickCount + ".png");
            }
        }

        private static TextureData LoadStoneTextureData()
        {
            return new ImageProcessorTexture(Path.Combine(AppContext.BaseDirectory, "Textures/CubeTexture.png"));
        }

        private static TextureData LoadWhiteTextureData()
        {
            var texture = new RawTextureDataArray<RgbaFloat>(1, 1, RgbaFloat.SizeInBytes, Graphics.PixelFormat.R32_G32_B32_A32_Float);
            texture.PixelData[0] = RgbaFloat.White;
            return texture;
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