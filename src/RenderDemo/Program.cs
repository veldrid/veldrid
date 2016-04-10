using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
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
        private static bool _limitFrameRate = true;
        private static FlatListVisibilityManager _visiblityManager;
        private static ConstantDataProvider<DirectionalLightBuffer> _lightBufferProvider;
        private static DynamicDataProvider<Matrix4x4> _projectionMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Matrix4x4> _viewMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Matrix4x4> _lightViewMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Matrix4x4> _lightProjMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Vector4> _lightInfoProvider = new DynamicDataProvider<Vector4>();
        private static FlatListVisibilityManager _boxSceneVM;
        private static FlatListVisibilityManager _teapotVM;
        private static FlatListVisibilityManager _shadowsScene;
        private static double _circleWidth = 5.0;
        private static bool _wireframe;

        private static bool _takeScreenshot;
        private static ImGuiRenderer _imguiRenderer;

        private static Framebuffer _screenshotFramebuffer;
        private static RasterizerState _wireframeRasterizerState;

        private static Vector3 _cameraPosition;
        private static float _cameraYaw;
        private static float _cameraPitch;

        private static Vector3 _lightDirection;
        private static float _fieldOfViewRadians = 1.05f;
        private static bool _autoRotateCamera = false;
        private static bool _moveLight = false;

        private static float _previousMouseX;
        private static float _previousMouseY;
        private static float _cameraMoveSpeed = 7.5f;
        private static float _cameraSprintFactor = 2.5f;

        private static bool _onWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static void Main()
        {
            try
            {
                if (_onWindows)
                {
                    _window = new DedicatedThreadWindow();
                }
                else
                {
                    _window = new SameThreadWindow();
                }

                bool preferD3D = _onWindows;
                if (preferD3D)
                {
                    _rc = new D3DRenderContext(_window);
                }
                else
                {
                    _rc = new OpenGLRenderContext(_window);
                }

                _renderer = new Renderer(_rc, new PipelineStage[]
                {
                    new ShadowMapStage(_rc),
                    new StandardPipelineStage("Standard"),
                    new StandardPipelineStage("Overlay"),
                });

                _imguiRenderer = new ImGuiRenderer(_rc, _window.NativeWindow);

                _lightBufferProvider = new ConstantDataProvider<DirectionalLightBuffer>(
                    new DirectionalLightBuffer(RgbaFloat.White, new Vector3(-.3f, -1f, -1f)));

                // Shader buffers for shadow mapping
                _lightDirection = Vector3.Normalize(new Vector3(1f, -1f, 0f));
                Vector3 lightPosition = -_lightDirection * 20f;

                _lightViewMatrixProvider.Data = Matrix4x4.CreateLookAt(lightPosition, Vector3.Zero, Vector3.UnitY);

                _lightProjMatrixProvider.Data = Matrix4x4.CreateOrthographicOffCenter(-18, 18, -18, 18, -10, 60f);

                _lightInfoProvider.Data = new Vector4(_lightDirection, 1);

                float timeFactor = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
                _cameraPosition = new Vector3(
                    (float)(Math.Cos(timeFactor) * _circleWidth),
                    3 + (float)Math.Sin(timeFactor) * 2,
                    (float)(Math.Sin(timeFactor) * _circleWidth));
                var cameraRotation = Matrix4x4.CreateLookAt(_cameraPosition, -_cameraPosition, Vector3.UnitY);
                SetCameraLookMatrix(cameraRotation);

                _rc.DataProviders.Add("LightBuffer", _lightBufferProvider);
                _rc.DataProviders.Add("ProjectionMatrix", _projectionMatrixProvider);
                _rc.DataProviders.Add("ViewMatrix", _viewMatrixProvider);
                _rc.DataProviders.Add("LightViewMatrix", _lightViewMatrixProvider);
                _rc.DataProviders.Add("LightProjMatrix", _lightProjMatrixProvider);
                _rc.DataProviders.Add("LightInfo", _lightInfoProvider);
                _rc.ClearColor = RgbaFloat.CornflowerBlue;

                _rc.WindowResized += OnWindowResized;
                SetProjectionMatrix();

                CreateScreenshotFramebuffer();
                CreateWireframeRasterizerState();

                _visiblityManager = SceneWithShadows();

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
                    InputTracker.UpdateFrameInput(snapshot);
                    Update(deltaMilliseconds);
                    Draw();
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

        private static void SetCameraLookMatrix(Matrix4x4 mat)
        {
            _viewMatrixProvider.Data = mat;
        }

        private static void OnWindowResized()
        {
            SetProjectionMatrix();
        }

        private static void SetProjectionMatrix()
        {
            _projectionMatrixProvider.Data = Matrix4x4.CreatePerspectiveFieldOfView(
                _fieldOfViewRadians,
                _rc.Window.Width / (float)_rc.Window.Height,
                1f,
                1000f);
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
                var tcr = new TexturedMeshRenderer(_rc, sphere.Vertices, sphere.Indices, Textures.CubeTexture);
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
                teapot.Scale = new Vector3(1f);

                var plane = new TexturedMeshRenderer(_rc, PlaneModel.Vertices, PlaneModel.Indices, Textures.WoodTexture);
                plane.Position = new Vector3(0, -2, 0);
                plane.Scale = new Vector3(20, 1, 20);

                _teapotVM.AddRenderItem(teapot);
                _teapotVM.AddRenderItem(plane);
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

                var cube1 = new ShadowCaster(_rc, CubeModel.Vertices, CubeModel.Indices, Textures.CubeTexture);
                _shadowsScene.AddRenderItem(cube1);

                var cube2 = new ShadowCaster(_rc, CubeModel.Vertices, CubeModel.Indices, Textures.CubeTexture);
                cube2.Position = new Vector3(3f, 5f, 0f);
                cube2.Scale = new Vector3(3f);
                _shadowsScene.AddRenderItem(cube2);

                var sphere3 = new ShadowCaster(_rc, sphereMeshInfo.Vertices, sphereMeshInfo.Indices, Textures.PureWhiteTexture);
                sphere3.Position = new Vector3(0f, 0f, 5f);
                _shadowsScene.AddRenderItem(sphere3);

                var plane = new ShadowCaster(_rc, PlaneModel.Vertices, PlaneModel.Indices, Textures.WoodTexture);
                plane.Position = new Vector3(0, -2.5f, 0);
                plane.Scale = new Vector3(20f);

                var shadowMapPreview = new ShadowMapPreview(_rc);
                _shadowsScene.AddRenderItem(shadowMapPreview);

                _shadowsScene.AddRenderItem(plane);
                _shadowsScene.AddRenderItem(_imguiRenderer);
            }

            return _shadowsScene;
        }

        private static void Update(double deltaMilliseconds)
        {
            float timeFactor = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            if (_autoRotateCamera)
            {
                _cameraPosition = new Vector3(
                    (float)(Math.Cos(timeFactor) * _circleWidth),
                    3 + (float)Math.Sin(timeFactor) * 2,
                    (float)(Math.Sin(timeFactor) * _circleWidth));
                var cameraRotationMat = Matrix4x4.CreateLookAt(_cameraPosition, -_cameraPosition, Vector3.UnitY);
                SetCameraLookMatrix(cameraRotationMat);
            }

            if (_moveLight)
            {
                var position = new Vector3(
                    (float)(Math.Cos(timeFactor) * _circleWidth),
                    3 + (float)Math.Sin(timeFactor) * 2,
                    (float)(Math.Sin(timeFactor) * _circleWidth));

                _lightViewMatrixProvider.Data = Matrix4x4.CreateLookAt(position, Vector3.Zero, Vector3.UnitY);
                _lightInfoProvider.Data = new Vector4(-position, 1);
            }

            _imguiRenderer.SetPerFrameImGuiData(_rc, (float)deltaMilliseconds);
            _imguiRenderer.UpdateImGuiInput(_window);
            _imguiRenderer.NewFrame();

            bool opened = false;
            float width = Math.Max(100, Math.Min(200, _window.Width * .4f));
            ImGui.SetNextWindowPos(new Vector2(20, 20), SetCondition.Always);
            if (ImGui.BeginWindow("Scenes", ref opened, new Vector2(width, 360), 0.8f, WindowFlags.NoMove | WindowFlags.NoResize))
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

                if (ImGui.SliderFloat("FOV", ref _fieldOfViewRadians, 0.05f, (float)Math.PI - .01f, _fieldOfViewRadians.ToString(), 1f))
                {
                    SetProjectionMatrix();
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

                string cameraLabel = _autoRotateCamera ? "Camera: Auto" : "Camera: WASD";
                bool colorLabel = _autoRotateCamera;
                if (colorLabel)
                {
                    ImGui.PushStyleColor(ColorTarget.Button, RgbaFloat.Cyan.ToVector4());
                    ImGui.PushStyleColor(ColorTarget.ButtonHovered, RgbaFloat.Cyan.ToVector4());
                }
                if (ImGui.Button(cameraLabel))
                {
                    _autoRotateCamera = !_autoRotateCamera;
                }
                if (colorLabel)
                {
                    ImGui.PopStyleColor(2);
                }

                ImGui.Checkbox("Auto-Rotate Light", ref _moveLight);

                bool isD3D11 = _rc is D3DRenderContext;
                bool isOpenGL = !isD3D11;

                if (_onWindows)
                {
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

            if (InputTracker.GetKeyDown(OpenTK.Input.Key.F4))
            {
                ((ShadowMapStage)_renderer.Stages[0]).TakeScreenshot = true;
            }
            if (InputTracker.GetKeyDown(OpenTK.Input.Key.F11))
            {
                _window.WindowState = _window.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
            }
            if (InputTracker.GetKeyDown(OpenTK.Input.Key.Escape))
            {
                _window.Close();
            }

            float deltaX = InputTracker.MousePosition.X - _previousMouseX;
            float deltaY = InputTracker.MousePosition.Y - _previousMouseY;
            _previousMouseX = InputTracker.MousePosition.X;
            _previousMouseY = InputTracker.MousePosition.Y;

            Quaternion cameraRotation = Quaternion.CreateFromYawPitchRoll(_cameraYaw, _cameraPitch, 0f);
            Vector3 cameraForward = Vector3.Transform(-Vector3.UnitZ, cameraRotation);
            Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(cameraForward, Vector3.UnitY));
            Vector3 cameraUp = Vector3.Normalize(Vector3.Cross(cameraRight, cameraForward));

            float deltaSec = (float)deltaMilliseconds / 1000f;
            bool cameraMoved = false;

            if ((InputTracker.GetMouseButton(OpenTK.Input.MouseButton.Left) || InputTracker.GetMouseButton(OpenTK.Input.MouseButton.Right)) && !_autoRotateCamera)
            {
                cameraMoved = true;
                if (!InputTracker.GetMouseButtonDown(OpenTK.Input.MouseButton.Left) && !InputTracker.GetMouseButtonDown(OpenTK.Input.MouseButton.Right))
                {
                    _cameraYaw += -deltaX * .01f;
                    _cameraPitch += -deltaY * .01f;

                    float sprintFactor = InputTracker.GetKey(OpenTK.Input.Key.LShift) ? _cameraSprintFactor : 1.0f;
                    if (InputTracker.GetKey(OpenTK.Input.Key.W))
                    {
                        _cameraPosition += cameraForward * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                    if (InputTracker.GetKey(OpenTK.Input.Key.S))
                    {
                        _cameraPosition -= cameraForward * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                    if (InputTracker.GetKey(OpenTK.Input.Key.D))
                    {
                        _cameraPosition += cameraRight * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                    if (InputTracker.GetKey(OpenTK.Input.Key.A))
                    {
                        _cameraPosition -= cameraRight * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                    if (InputTracker.GetKey(OpenTK.Input.Key.E))
                    {
                        _cameraPosition += cameraUp * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                    if (InputTracker.GetKey(OpenTK.Input.Key.Q))
                    {
                        _cameraPosition -= cameraUp * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                }
            }

            if (InputTracker.GetMouseButton(OpenTK.Input.MouseButton.Middle) && !_autoRotateCamera)
            {
                if (!InputTracker.GetMouseButtonDown(OpenTK.Input.MouseButton.Middle))
                {
                    cameraMoved = true;
                    _cameraPosition += (deltaX * -cameraRight + deltaY * cameraUp) * .01f;
                }
            }

            if (cameraMoved)
            {
                Matrix4x4 cameraView = Matrix4x4.CreateLookAt(_cameraPosition, _cameraPosition + cameraForward, Vector3.UnitY);
                SetCameraLookMatrix(cameraView);
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
                foreach (var kvp in _rc.TextureProviders)
                {
                    newContext.TextureProviders[kvp.Key] = kvp.Value;
                }

                _renderer.SetRenderContext(newContext);

                if (_teapotVM != null)
                {
                    foreach (var item in _teapotVM?.RenderItems)
                    {
                        ((SwappableRenderItem)item).ChangeRenderContext(newContext);
                    }
                }
                if (_boxSceneVM != null)
                {
                    foreach (var item in _boxSceneVM.RenderItems)
                    {
                        ((SwappableRenderItem)item).ChangeRenderContext(newContext);
                    }
                }
                if (_shadowsScene != null)
                {
                    foreach (var item in _shadowsScene.RenderItems)
                    {
                        ((SwappableRenderItem)item).ChangeRenderContext(newContext);
                    }
                }

                _rc.Dispose();
                _rc = newContext;
                _rc.WindowResized += OnWindowResized;

                CreateWireframeRasterizerState();
                if (_wireframe)
                {
                    _rc.SetRasterizerState(_wireframeRasterizerState);
                }
            }
        }

        private unsafe static void Draw()
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