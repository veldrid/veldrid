using ImGuiNET;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Vd2;
using Vd2.ImageSharp;
using Vd2.NeoDemo.Objects;
using Vd2.StartupUtilities;
using Vd2.Utilities;
using VdSdl2;

namespace Vd2.NeoDemo
{
    public class NeoDemo
    {
        private Sdl2Window _window;
        private GraphicsDevice _gd;
        private Scene _scene;
        private readonly ImGuiRenderable _igRenderable;
        private readonly SceneContext _sc = new SceneContext();
        private bool _windowResized;
        private RenderOrderKeyComparer _renderOrderKeyComparer = new RenderOrderKeyComparer();
        private bool _recreateWindow = true;

        private static double _desiredFrameLengthSeconds = 1.0 / 60.0;
        private static bool _limitFrameRate = false;
        private static FrameTimeAverager _fta = new FrameTimeAverager(0.666);

        private event Action<int, int> _resizeHandled;

        public NeoDemo()
        {
            WindowCreateInfo windowCI = new WindowCreateInfo
            {
                X = 50,
                Y = 50,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowInitialState = WindowState.Normal,
                WindowTitle = "Vd NeoDemo"
            };
            GraphicsDeviceCreateInfo gdCI = new GraphicsDeviceCreateInfo();
            gdCI.Backend = GraphicsBackend.Vulkan;
#if DEBUG
            gdCI.DebugDevice = true;
#endif

            Vd2Startup.CreateWindowAndGraphicsDevice(ref windowCI, ref gdCI, out _window, out _gd);
            _window.Resized += () => _windowResized = true;

            CommandList initCL = _gd.ResourceFactory.CreateCommandList();
            initCL.Begin();

            _sc.CreateDeviceObjects(_gd, initCL, _sc);
            CommonMaterials.CreateAllDeviceObjects(_gd, initCL, _sc);

            _scene = new Scene(_window.Width, _window.Height);

            _sc.SetCurrentScene(_scene, initCL);

            _igRenderable = new ImGuiRenderable(_window.Width, _window.Height);
            _resizeHandled += (w, h) => _igRenderable.WindowResized(w, h);
            _igRenderable.CreateDeviceObjects(_gd, initCL, _sc);
            _scene.AddRenderable(_igRenderable);
            _scene.AddUpdateable(_igRenderable);

            InfiniteGrid grid = new InfiniteGrid();
            grid.CreateDeviceObjects(_gd, initCL, _sc);
            _scene.AddRenderable(grid);

            Skybox skybox = Skybox.LoadDefaultSkybox();
            skybox.CreateDeviceObjects(_gd, initCL, _sc);
            _scene.AddRenderable(skybox);

            AddTexturedMesh(
                initCL,
                "Textures/spnza_bricks_a_diff.png",
                PrimitiveShapes.Box(10, 10, 10, 10),
                new Vector3(0, 0, -5),
                Quaternion.Identity,
                Vector3.One);

            AddTexturedMesh(
                initCL,
                "Textures/spnza_bricks_a_diff.png",
                PrimitiveShapes.Box(5, 5, 5, 5f),
                new Vector3(-3, -9, 2),
                Quaternion.Identity,
                Vector3.One);

            AddTexturedMesh(
                initCL,
                "Textures/spnza_bricks_a_diff.png",
                PrimitiveShapes.Box(27, 3, 27, 27f),
                new Vector3(-5, -16, 5),
                Quaternion.Identity,
                Vector3.One);

            AddTexturedMesh(
                initCL,
                "Textures/spnza_bricks_a_diff.png",
                PrimitiveShapes.Plane(100, 100, 5),
                new Vector3(0, -20, 0),
                Quaternion.Identity,
                Vector3.One);

            ShadowmapDrawer texDrawer = new ShadowmapDrawer(() => _window, () => _sc.NearShadowMapView);
            _resizeHandled += (w, h) => texDrawer.OnWindowResized();
            texDrawer.CreateDeviceObjects(_gd, initCL, _sc);
            texDrawer.Position = new Vector2(10, 25);
            _scene.AddRenderable(texDrawer);

            ShadowmapDrawer texDrawer2 = new ShadowmapDrawer(() => _window, () => _sc.MidShadowMapView);
            _resizeHandled += (w, h) => texDrawer2.OnWindowResized();
            texDrawer2.CreateDeviceObjects(_gd, initCL, _sc);
            texDrawer2.Position = new Vector2(20 + texDrawer2.Size.X, 25);
            _scene.AddRenderable(texDrawer2);

            ShadowmapDrawer texDrawer3 = new ShadowmapDrawer(() => _window, () => _sc.FarShadowMapView);
            _resizeHandled += (w, h) => texDrawer3.OnWindowResized();
            texDrawer3.CreateDeviceObjects(_gd, initCL, _sc);
            texDrawer3.Position = new Vector2(30 + (texDrawer3.Size.X * 2), 25);
            _scene.AddRenderable(texDrawer3);

            initCL.End();
            _gd.ExecuteCommands(initCL);
            initCL.Dispose();
        }

        private void AddTexturedMesh(CommandList cl, string texPath, MeshData meshData, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            ImageSharpTexture texData = new ImageSharpTexture(AssetHelper.GetPath(texPath));
            TexturedMesh mesh = new TexturedMesh(meshData, texData, CommonMaterials.Brick);
            mesh.Transform.Position = position;
            mesh.Transform.Rotation = rotation;
            mesh.Transform.Scale = scale;
            mesh.CreateDeviceObjects(_gd, cl, _sc);
            _scene.AddRenderable(mesh);
        }

        public void Run()
        {
            long previousFrameTicks = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (_window.Exists)
            {
                long currentFrameTicks = sw.ElapsedTicks;
                double deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

                while (_limitFrameRate && deltaSeconds < _desiredFrameLengthSeconds)
                {
                    currentFrameTicks = sw.ElapsedTicks;
                    deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                }

                previousFrameTicks = currentFrameTicks;

                InputSnapshot snapshot = _window.PumpEvents();
                InputTracker.UpdateFrameInput(snapshot);
                Update((float)deltaSeconds);
                Draw();
            }
        }

        private void Update(float deltaSeconds)
        {
            _fta.AddTime(deltaSeconds);
            _scene.Update(deltaSeconds);

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Settings"))
                {
                    if (ImGui.BeginMenu("Graphics Backend"))
                    {
                        if (ImGui.MenuItem("Vulkan"))
                        {
                            ChangeBackend(GraphicsBackend.Vulkan);
                        }
                        if (ImGui.MenuItem("OpenGL"))
                        {
                            ChangeBackend(GraphicsBackend.OpenGL);
                        }
                        if (ImGui.MenuItem("OpenGL ES"))
                        {
                            ChangeBackend(GraphicsBackend.OpenGLES);
                        }
                        if (ImGui.MenuItem("Direct3D 11"))
                        {
                            ChangeBackend(GraphicsBackend.Direct3D11);
                        }
                        ImGui.EndMenu();
                    }
                    bool isFullscreen = _window.WindowState == WindowState.BorderlessFullScreen;
                    if (ImGui.MenuItem("Fullscreen", "F11", isFullscreen, true))
                    {
                        ToggleFullscreenState();
                    }

                    if (ImGui.MenuItem("Always Recreate Sdl2Window", string.Empty, _recreateWindow, true))
                    {
                        _recreateWindow = !_recreateWindow;
                    }
                    if (ImGui.IsLastItemHovered())
                    {
                        ImGui.SetTooltip(
                            "Causes a new OS Sdl2Window to be created whenever the graphics backend is switched. This is much safer, and is the default.");
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Materials"))
                {
                    if (ImGui.BeginMenu("Brick"))
                    {
                        DrawMaterialMenu(CommonMaterials.Brick);
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }

                ImGui.Text(_fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms"));

                ImGui.EndMainMenuBar();
            }

            if (InputTracker.GetKeyDown(Key.F11))
            {
                ToggleFullscreenState();
            }

            _window.Title = _gd.BackendType.ToString();
        }

        private void DrawMaterialMenu(MaterialPropsAndBuffer brick)
        {
            MaterialProperties props = brick.Properties;
            float intensity = props.SpecularIntensity.X;
            if (ImGui.SliderFloat("Intensity", ref intensity, 0f, 10f, intensity.ToString(), 1f)
                | ImGui.SliderFloat("Power", ref props.SpecularPower, 0f, 1000f, props.SpecularPower.ToString(), 1f))
            {
                props.SpecularIntensity = new Vector3(intensity);
                brick.Properties = props;
            }
        }

        private void ToggleFullscreenState()
        {
            bool isFullscreen = _window.WindowState == WindowState.BorderlessFullScreen;
            _window.WindowState = isFullscreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
        }

        private void Draw()
        {
            int width = _window.Width;
            int height = _window.Height;

            if (_windowResized && _window.Exists)
            {
                _windowResized = false;
                _gd.ResizeMainWindow((uint)width, (uint)height);
                _scene.Camera.WindowResized(width, height);
                _resizeHandled?.Invoke(width, height);
            }

            CommandList cl = _gd.ResourceFactory.CreateCommandList();
            cl.Begin();
            CommonMaterials.FlushAll(cl);
            _sc.UpdateCameraBuffers(cl); // Meh

            _scene.RenderAllStages(_gd, cl, _sc);
            cl.End();
            _gd.ExecuteCommands(cl);
            cl.Dispose();

            if (_window.Exists)
            {
                _gd.SwapBuffers();
            }
        }

        private void ChangeBackend(GraphicsBackend backend)
        {
            _sc.DestroyDeviceObjects();
            _scene.DestroyAllDeviceObjects();
            CommonMaterials.DestroyAllDeviceObjects();

            _gd.Dispose();

            if (_recreateWindow)
            {

                WindowCreateInfo windowCI = new WindowCreateInfo
                {
                    X = _window.X,
                    Y = _window.Y,
                    WindowWidth = _window.Width,
                    WindowHeight = _window.Height,
                    WindowInitialState = _window.WindowState,
                    WindowTitle = "Vd NeoDemo"
                };

                _window.Close();

                _window = Vd2Startup.CreateWindow(ref windowCI);
                _window.Resized += () => _windowResized = true;
            }

            GraphicsDeviceCreateInfo rcCI = new GraphicsDeviceCreateInfo
            {
                Backend = backend,
#if DEBUG
                DebugDevice = true
#endif
            };

            _gd = Vd2Startup.CreateGraphicsDevice(ref rcCI, _window);

            CommandList initCL = _gd.ResourceFactory.CreateCommandList();
            initCL.Begin();
            _sc.CreateDeviceObjects(_gd, initCL, _sc);
            CommonMaterials.CreateAllDeviceObjects(_gd, initCL, _sc);
            _scene.CreateAllDeviceObjects(_gd, initCL, _sc);
            initCL.End();
            _gd.ExecuteCommands(initCL);
            initCL.Dispose();
        }
    }
}
