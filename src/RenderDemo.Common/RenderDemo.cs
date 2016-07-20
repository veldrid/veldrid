using ImGuiNET;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid.Assets;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Graphics.Pipeline;
using Veldrid.Platform;
using Veldrid.RenderDemo.ForwardRendering;
using Veldrid.RenderDemo.Models;
using System.Collections.Generic;
using SharpDX.DXGI;
using System.Threading.Tasks;

namespace Veldrid.RenderDemo
{
    public static class RenderDemo
    {
        private static Renderer _renderer;
        private static RenderContext _rc;
        private static ShadowMapStage _shadowMapStage;
        private static RendererOption[] _backendOptions;
        private static FrameTimeAverager _fta;
        private static double _desiredFrameLengthMilliseconds = 1000.0 / 60.0;
        private static bool _limitFrameRate = true;
        private static VisibiltyManager _visibilityManager;
        private static ConstantDataProvider<DirectionalLightBuffer> _lightBufferProvider;
        private static DynamicDataProvider<Matrix4x4> _lightViewMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Matrix4x4> _lightProjMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Vector4> _lightInfoProvider = new DynamicDataProvider<Vector4>();
        private static FlatListVisibilityManager _boxSceneVM;
        private static FlatListVisibilityManager _teapotVM;
        private static OctreeVisibilityManager _shadowsScene;
        private static FlatListVisibilityManager _octreeScene;
        private static OctreeVisibilityManager _sponzaAtrium;
        private static double _circleWidth = 12.0;
        private static bool _wireframe;

        private static bool _takeScreenshot;
        private static ImGuiRenderer _imguiRenderer;

        private static Framebuffer _screenshotFramebuffer;
        private static RasterizerState _wireframeRasterizerState;
        private static MaterialAsset s_mtlMaterialAsset;

        private static Camera _camera;
        private static float _cameraYaw;
        private static float _cameraPitch;

        private static Vector3 _lightDirection;
        private static bool _autoRotateCamera = true;
        private static bool _moveLight = false;

        private static float _previousMouseX;
        private static float _previousMouseY;
        private static float _cameraMoveSpeed = 7.5f;
        private static float _cameraSprintFactor = 2.5f;

        private static bool _onWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static bool _preferencesEditorOpened;
        private static PipelineStage[] _configurableStages;

        private static LooseFileDatabase _ad;
        private static AssetEditorWindow _editorWindow;

        private static ShadowMapPreview _shadowMapPreview;
        private static RendererOption _selectedOption;
        private static OctreeNode<ShadowCaster> _octree;
        private static List<RenderItem> _octreeCubes = new List<RenderItem>();
        private static Vector3 _octreeBoxPosition;
        private static MaterialAsset _stoneMaterial;
        private static OctreeRenderer<ShadowCaster> _octreeRenderer;
        private static FrustumWireframeRenderer _octreeFrustumRenderer;
        private static Vector3 _octreeFrustumViewOrigin;
        private static Vector3 _octreeFrustumViewDirection = -Vector3.UnitZ;
        private static float _octreeFrustumNearDistance = 3f;
        private static float _octreeFrustumFarDistance = 18f;
        private static List<ShadowCaster> _octreeQueryResult = new List<ShadowCaster>();
        private static List<RenderItem> _sponzaQueryResult = new List<RenderItem>();
        private static BoundingBoxWireframeRenderer _sceneBoundsRenderer;

        public static void RunDemo(RenderContext renderContext, params RendererOption[] backendOptions)
        {
            try
            {
                _rc = renderContext;
                _backendOptions = backendOptions;
                _selectedOption = backendOptions.FirstOrDefault();

                _shadowMapStage = new ShadowMapStage(_rc);
                _configurableStages = new PipelineStage[]
                {
                    _shadowMapStage,
                    new StandardPipelineStage(_rc, "Standard"),
                    new StandardPipelineStage(_rc, "Overlay"),
                };

                _renderer = new Renderer(_rc, _configurableStages.Concat(new[] { new StandardPipelineStage(_rc, "ImGui") }).ToArray());

                _ad = new LooseFileDatabase(Path.Combine(AppContext.BaseDirectory, "Assets"));
                _editorWindow = new AssetEditorWindow(_ad);

                _imguiRenderer = new ImGuiRenderer(_rc);

                _lightBufferProvider = new ConstantDataProvider<DirectionalLightBuffer>(
                    new DirectionalLightBuffer(RgbaFloat.White, new Vector3(-.3f, -1f, -1f)));

                // Shader buffers for shadow mapping
                _lightDirection = Vector3.Normalize(new Vector3(1f, -1f, 0f));
                _lightInfoProvider.Data = new Vector4(_lightDirection, 1);

                _camera = new Camera(_rc.Window);
                float timeFactor = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
                _camera.Position = new Vector3(
                    (float)(Math.Cos(timeFactor) * _circleWidth),
                    3 + (float)Math.Sin(timeFactor) * 2,
                    (float)(Math.Sin(timeFactor) * _circleWidth));
                _camera.LookDirection = -_camera.Position;

                _rc.RegisterGlobalDataProvider("ProjectionMatrix", _camera.ProjectionProvider);
                _rc.RegisterGlobalDataProvider("ViewMatrix", _camera.ViewProvider);
                _rc.RegisterGlobalDataProvider("LightBuffer", _lightBufferProvider);
                _rc.RegisterGlobalDataProvider("LightViewMatrix", _lightViewMatrixProvider);
                _rc.RegisterGlobalDataProvider("LightProjMatrix", _lightProjMatrixProvider);
                _rc.RegisterGlobalDataProvider("LightInfo", _lightInfoProvider);
                _rc.RegisterGlobalDataProvider("CameraInfo", _camera.CameraInfoProvider);
                _rc.ClearColor = RgbaFloat.CornflowerBlue;

                UpdateLightMatrices();

                CreateScreenshotFramebuffer();
                CreateWireframeRasterizerState();

                ChangeScene(SceneWithShadows());

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
                        currentFrameTicks = sw.ElapsedTicks;
                        deltaMilliseconds = (currentFrameTicks - previousFrameTicks) * (1000.0 / Stopwatch.Frequency);
                    }

                    previousFrameTicks = currentFrameTicks;

                    var snapshot = _rc.Window.GetInputSnapshot();
                    InputTracker.UpdateFrameInput(snapshot);
                    Update(deltaMilliseconds, snapshot);
                    Draw();
                }

            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                Console.WriteLine("Error: " + e);
                if (_rc is OpenGLRenderContext)
                {
                    //Console.WriteLine("GL Error: " + GL.GetError());
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
            _screenshotFramebuffer = _rc.ResourceFactory.CreateFramebuffer(_rc.Window.Width, _rc.Window.Height);
        }

        private static FlatListVisibilityManager SceneWithBoxes()
        {
            if (_boxSceneVM == null)
            {
                _boxSceneVM = new FlatListVisibilityManager();
                var sphere = _ad.LoadAsset<ObjFile>(new AssetID("Models/Sphere.obj")).GetFirstMesh();
                var tcr = new TexturedMeshRenderer(_ad, _rc, sphere.Vertices, sphere.Indices, Textures.CubeTexture);
                tcr.Position = new Vector3(-5f, 0, -3);
                _boxSceneVM.AddRenderItem(tcr);

                for (int x = 0; x < 6; x++)
                {
                    for (int y = 0; y < 6; y++)
                    {
                        for (int z = 0; z < 6; z++)
                        {
                            var ccr = new ColoredCubeRenderer(_ad, _rc);
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

                var plane = new TexturedMeshRenderer(_ad, _rc, PlaneModel.Vertices, PlaneModel.Indices, Textures.WoodTexture);
                plane.Position = new Vector3(0, -2, 0);
                plane.Scale = new Vector3(20, 1, 20);

                _teapotVM.AddRenderItem(teapot);
                _teapotVM.AddRenderItem(plane);
                _teapotVM.AddRenderItem(_imguiRenderer);
            }

            return _teapotVM;
        }

        private static OctreeVisibilityManager SceneWithShadows()
        {
            if (_shadowsScene == null)
            {
                _shadowsScene = new OctreeVisibilityManager();

                var stoneMaterial = _ad.LoadAsset<MaterialAsset>(new AssetID("MaterialAsset/ShadowCaster_Stone.json"));
                var woodMaterial = _ad.LoadAsset<MaterialAsset>(new AssetID("MaterialAsset/ShadowCaster_Wood.json"));
                var crateMaterial = _ad.LoadAsset<MaterialAsset>(new AssetID("MaterialAsset/ShadowCaster_Crate.json"));

                var cube1 = new ShadowCaster(_rc, _ad, CubeModel.Vertices, CubeModel.Indices, crateMaterial);
                _shadowsScene.AddRenderItem(cube1.BoundingBox, cube1);

                var cube2 = new ShadowCaster(_rc, _ad, CubeModel.Vertices, CubeModel.Indices, crateMaterial);
                cube2.Position = new Vector3(3f, 5f, 0f);
                cube2.Scale = new Vector3(3f);
                _shadowsScene.AddRenderItem(cube2.BoundingBox, cube2);

                var teapot = new ShadowCaster(_rc, _ad, CubeModel.Vertices, CubeModel.Indices, stoneMaterial);
                teapot.Position = new Vector3(-4f, 0f, 6f);
                teapot.Rotation = Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateRotationY(1));
                _shadowsScene.AddRenderItem(teapot.BoundingBox, teapot);

                var plane = new ShadowCaster(_rc, _ad, PlaneModel.Vertices, PlaneModel.Indices, woodMaterial);
                plane.Position = new Vector3(0, -2.5f, 0);
                plane.Scale = new Vector3(20f);
                _shadowsScene.AddRenderItem(plane.BoundingBox, plane);

                _sceneBoundsRenderer = new BoundingBoxWireframeRenderer(_shadowsScene.OctreeRootNode.GetPreciseBounds(), _ad, _rc);
                _shadowsScene.AddRenderItem(_sceneBoundsRenderer);

                var skybox = new Skybox(_rc, _ad);
                _shadowsScene.AddRenderItem(skybox);

                _shadowMapPreview = new ShadowMapPreview(_ad, _rc);
                _shadowsScene.AddRenderItem(_shadowMapPreview);

                _shadowsScene.AddRenderItem(_imguiRenderer);
            }

            return _shadowsScene;
        }

        private static FlatListVisibilityManager OctreeScene()
        {
            if (_octreeScene == null)
            {
                _octreeScene = new FlatListVisibilityManager();
                _stoneMaterial = _ad.LoadAsset<MaterialAsset>(new AssetID("MaterialAsset/ShadowCaster_Stone.json"));

                BoundingBox bounds = new BoundingBox(new Vector3(-25, -25, -25), new Vector3(25, 25, 25));
                _octree = Octree.CreateNewTree<ShadowCaster>(ref bounds, 3);
                _octreeRenderer = new OctreeRenderer<ShadowCaster>(_octree, _ad, _rc);
                _octreeScene.AddRenderItem(_octreeRenderer);

                _octreeFrustumRenderer = new FrustumWireframeRenderer(default(BoundingFrustum), _ad, _rc);
                RecreateOctreeFrustum();
                _octreeScene.AddRenderItem(_octreeFrustumRenderer);

                AddRandomOctreeItems();

                _octreeScene.AddRenderItem(_imguiRenderer);
            }

            return _octreeScene;
        }

        private static void AddRandomOctreeItems()
        {
            Random r = new Random();
            for (int i = 0; i < 50; i++)
            {
                Vector3 position = new Vector3((float)r.NextDouble() * 48f - 24f, (float)r.NextDouble() * 48f - 24f, (float)r.NextDouble() * 48f - 24f);
                AddOctreeCube(position);
            }
        }

        private static void ClearOctreeItems()
        {
            _octree.Clear();
            foreach (var item in _octreeCubes)
            {
                _octreeScene.RemoveRenderItem(item);
            }
            _octreeCubes.Clear();
        }

        private static void AddOctreeCube(Vector3 position)
        {
            var cube = new ShadowCaster(_rc, _ad, CubeModel.Vertices, CubeModel.Indices, _stoneMaterial);
            cube.Position = position;
            _octreeScene.AddRenderItem(cube);
            _octree = _octree.AddItem(cube.BoundingBox, cube);
            _octreeRenderer.Octree = _octree;
            _octreeCubes.Add(cube);
        }

        private static OctreeVisibilityManager SponzaAtriumScene()
        {
            if (_sponzaAtrium == null)
            {
                _sponzaAtrium = new OctreeVisibilityManager();

                ObjFile atriumFile = _ad.LoadAsset<ObjFile>("Models/SponzaAtrium/sponza.obj", cache: false);
                MtlFile atriumMtls;
                using (var mtlStream = _ad.OpenAssetStream("Models/SponzaAtrium/sponza.mtl"))
                {
                    atriumMtls = new MtlParser().Parse(mtlStream);
                }

                var sphere = _ad.LoadAsset<ObjFile>(new AssetID("Models/Sphere.obj")).GetFirstMesh();
                var pink = new RawTextureDataArray<RgbaFloat>(
                    new RgbaFloat[] { RgbaFloat.Pink }, 1, 1, RgbaFloat.SizeInBytes, PixelFormat.R32_G32_B32_A32_Float);
                foreach (var group in atriumFile.MeshGroups)
                {
                    ConstructedMeshInfo mesh = atriumFile.GetMesh(group);
                    MaterialDefinition materialDef = atriumMtls.Definitions[mesh.MaterialName];
                    MaterialAsset matAsset = GetMtlMaterialAssetTemplate();
                    TextureData overrideTextureData = null;
                    if (materialDef.DiffuseTexture != null)
                    {
                        string texturePath = "Models/SponzaAtrium/" + materialDef.DiffuseTexture;
                        overrideTextureData = _ad.LoadAsset<ImageProcessorTexture>(texturePath);
                    }
                    else
                    {
                        overrideTextureData = pink;
                    }

                    MtlShadowCaster sc = new MtlShadowCaster(_rc, _ad, mesh.Vertices, mesh.Indices, matAsset, overrideTextureData);
                    sc.Scale = new Vector3(0.1f);
                    sc.Name = group.Name + ":" + group.Material;
                    Vector3 specularIntensity = Vector3.Zero;
                    if (materialDef.Name.Contains("vase"))
                    {
                        specularIntensity = new Vector3(1.0f);
                    }
                    else
                    {
                        specularIntensity = new Vector3(0.2f);
                    }

                    sc.MaterialProperties = new MtlMaterialProperties(specularIntensity, materialDef.SpecularExponent);

                    _sponzaAtrium.AddRenderItem(sc.BoundingBox, sc);

                    // This renders the bounding boxes of the atrium meshes.
                    //BoundingBoxWireframeRenderer boundsRenderer = new BoundingBoxWireframeRenderer(sc.BoundingBox, _ad, _rc);
                    //_sponzaAtrium.AddRenderItem(sc.BoundingBox, boundsRenderer);
                }

                //_sponzaAtrium.AddRenderItem(new OctreeRenderer<RenderItem>(_sponzaAtrium.Octree, _ad, _rc));

                var skybox = new Skybox(_rc, _ad);
                _sponzaAtrium.AddRenderItem(skybox);

                _sponzaAtrium.AddRenderItem(_imguiRenderer);

                _sceneBoundsRenderer = new BoundingBoxWireframeRenderer(_sponzaAtrium.OctreeRootNode.GetPreciseBounds(), _ad, _rc);
                _sponzaAtrium.AddRenderItem(_sceneBoundsRenderer);
            }

            return _sponzaAtrium;
        }

        private static MaterialAsset GetMtlMaterialAssetTemplate()
        {
            if (s_mtlMaterialAsset == null)
            {
                s_mtlMaterialAsset = _ad.LoadAsset<MaterialAsset>(new AssetID("MaterialAsset/ShadowCaster_MtlTemplate.json"));
            }

            return s_mtlMaterialAsset;
        }

        private static void Update(double deltaMilliseconds, InputSnapshot snapshot)
        {
            float timeFactor = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            if (_autoRotateCamera)
            {
                _camera.Position = new Vector3(
                    (float)(Math.Cos(timeFactor) * _circleWidth),
                    6 + (float)Math.Sin(timeFactor) * 2,
                    (float)(Math.Sin(timeFactor) * _circleWidth));
                _camera.LookDirection = -_camera.Position;
            }

            if (_moveLight)
            {
                var position = new Vector3(
                    (float)(Math.Cos(timeFactor) * 5),
                    6 + (float)Math.Sin(timeFactor) * 2,
                    -(float)(Math.Sin(timeFactor) * 5));

                _lightDirection = Vector3.Normalize(-position);
            }

            _imguiRenderer.SetPerFrameImGuiData(_rc, (float)deltaMilliseconds);
            _imguiRenderer.UpdateImGuiInput(_rc.Window, snapshot);

            DrawMainMenu();

            _fta.AddTime(deltaMilliseconds);

            string apiName = (_rc is OpenGLRenderContext) ? "OpenGL" : "Direct3D";
            _rc.Window.Title = $"[{apiName}] " + _fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTime.ToString("#00.00 ms");

            if (InputTracker.GetKeyDown(Key.F4) && (InputTracker.GetKey(Key.AltLeft) || InputTracker.GetKey(Key.AltRight)))
            {
                _rc.Window.Close();
            }
            if (InputTracker.GetKeyDown(Key.PrintScreen))
            {
                ((ShadowMapStage)_renderer.Stages[0]).SaveNextFrame();
            }
            if (InputTracker.GetKeyDown(Key.F11))
            {
                ToggleFullScreenState();
            }
            if (_visibilityManager == _octreeScene)
            {
                if (InputTracker.GetKeyDown(Key.F5))
                {
                    AddRandomOctreeItems();
                }
                if (InputTracker.GetKeyDown(Key.F6))
                {
                    ClearOctreeItems();
                }
                if (InputTracker.GetMouseButtonDown(MouseButton.Left))
                {
                    var screenPos = InputTracker.MousePosition;
                    Ray r = _camera.GetRayFromScreenPoint(screenPos.X, screenPos.Y);
                    _octreeQueryResult.Clear();
                    int numHits = _octree.RayCast(r, _octreeQueryResult);
                    Console.WriteLine("Hit " + numHits + " objects.");

                    foreach (var hit in _octreeQueryResult)
                    {
                        Console.WriteLine(hit.Name);
                    }
                }
                if (ImGui.BeginWindow("Octree Options"))
                {
                    ImGui.DragVector3("Box Position", ref _octreeBoxPosition, -24f, 24f, .3f);
                    if (ImGui.Button("Place Box"))
                    {
                        AddOctreeCube(_octreeBoxPosition);
                    }

                    if (ImGui.DragVector3("Frustum View Origin", ref _octreeFrustumViewOrigin, -60f, 60f, .1f)
                        | ImGui.DragVector3("Frustum View Direction", ref _octreeFrustumViewDirection, -60f, 60f, .1f)
                        | ImGui.DragFloat("Frustum Near Plane", ref _octreeFrustumNearDistance, 0.1f, 100f, .1f)
                        | ImGui.DragFloat("Frustum Far Plane", ref _octreeFrustumFarDistance, 0.1f, 100f, .1f))
                    {
                        RecreateOctreeFrustum();
                    }

                    _octreeQueryResult.Clear();
                    _octree.GetContainedObjects(_octreeFrustumRenderer.Frustum, _octreeQueryResult);
                    foreach (var item in _octreeQueryResult)
                    {
                        ImGui.Text(item.BoundingBox.ToString());
                    }
                }
                ImGui.EndWindow();
            }
            if (_visibilityManager == _sponzaAtrium)
            {
                if (InputTracker.GetMouseButtonDown(MouseButton.Left))
                {
                    var screenPos = InputTracker.MousePosition;
                    Ray r = _camera.GetRayFromScreenPoint(screenPos.X, screenPos.Y);
                    _sponzaQueryResult.Clear();
                    int numHits = _sponzaAtrium.OctreeRootNode.RayCast(r, _sponzaQueryResult);

                    var hits = _sponzaQueryResult.Where(ri => ri is ShadowCaster).Cast<ShadowCaster>().OrderBy(sc => Vector3.DistanceSquared(sc.BoundingBox.GetCenter(), _camera.Position));
                    Console.WriteLine("Hits: " + hits.Count());
                    var first = hits.FirstOrDefault();
                    if (first != null)
                    {
                        Console.WriteLine("First: " + first.Name);
                    }
                }
            }
            if (_visibilityManager == _shadowsScene || _visibilityManager == _sponzaAtrium)
            {
                _sceneBoundsRenderer.Box = ((OctreeVisibilityManager)_visibilityManager).OctreeRootNode.GetPreciseBounds();
            }

            UpdateLightMatrices();

            float deltaX = InputTracker.MousePosition.X - _previousMouseX;
            float deltaY = InputTracker.MousePosition.Y - _previousMouseY;
            _previousMouseX = InputTracker.MousePosition.X;
            _previousMouseY = InputTracker.MousePosition.Y;

            if (!_autoRotateCamera)
            {
                Quaternion cameraRotation = Quaternion.CreateFromYawPitchRoll(_cameraYaw, _cameraPitch, 0f);
                _camera.LookDirection = Vector3.Transform(-Vector3.UnitZ, cameraRotation);
            }

            Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(_camera.LookDirection, Vector3.UnitY));
            Vector3 cameraUp = Vector3.Normalize(Vector3.Cross(cameraRight, _camera.LookDirection));

            float deltaSec = (float)deltaMilliseconds / 1000f;

            if (!ImGui.IsMouseHoveringAnyWindow() && !ImGui.IsAnyItemActive() && !_autoRotateCamera
                && (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right)))
            {
                if (!InputTracker.GetMouseButtonDown(MouseButton.Left) && !InputTracker.GetMouseButtonDown(MouseButton.Right))
                {
                    _cameraYaw += -deltaX * .01f;
                    _cameraPitch += -deltaY * .01f;

                    float sprintFactor = InputTracker.GetKey(Key.LShift) ? _cameraSprintFactor : 1.0f;
                    sprintFactor = InputTracker.GetKey(Key.ControlLeft) ? (1f / (3 * _cameraSprintFactor)) : sprintFactor;
                    if (InputTracker.GetKey(Key.W))
                    {
                        _camera.Position += _camera.LookDirection * _cameraMoveSpeed * sprintFactor * deltaSec;
                        if (_camera.UseOrthographicProjection)
                        {
                            _camera.OrthographicWidth -= 5f * deltaSec * sprintFactor;
                        }
                    }
                    if (InputTracker.GetKey(Key.S))
                    {
                        _camera.Position -= _camera.LookDirection * _cameraMoveSpeed * sprintFactor * deltaSec;
                        if (_camera.UseOrthographicProjection)
                        {
                            _camera.OrthographicWidth += 5f * deltaSec * sprintFactor;
                        }
                    }
                    if (InputTracker.GetKey(Key.D))
                    {
                        _camera.Position += cameraRight * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                    if (InputTracker.GetKey(Key.A))
                    {
                        _camera.Position -= cameraRight * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                    if (InputTracker.GetKey(Key.E))
                    {
                        _camera.Position += cameraUp * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                    if (InputTracker.GetKey(Key.Q))
                    {
                        _camera.Position -= cameraUp * _cameraMoveSpeed * sprintFactor * deltaSec;
                    }
                }
            }

            if (InputTracker.GetMouseButton(MouseButton.Middle) && !_autoRotateCamera)
            {
                if (!InputTracker.GetMouseButtonDown(MouseButton.Middle))
                {
                    _camera.Position += (deltaX * -cameraRight + deltaY * cameraUp) * .01f;
                }
            }

            _editorWindow.Render(_rc);

            _imguiRenderer.UpdateFinished();
        }

        private static void RecreateOctreeFrustum()
        {
            _octreeFrustumRenderer.Frustum = new BoundingFrustum(
                Matrix4x4.CreateLookAt(
                    _octreeFrustumViewOrigin,
                    _octreeFrustumViewOrigin + Vector3.Normalize(_octreeFrustumViewDirection),
                    Vector3.UnitY)
                * Matrix4x4.CreatePerspectiveFieldOfView(1.0f, 1.0f, _octreeFrustumNearDistance, _octreeFrustumFarDistance));
        }

        private static void UpdateLightMatrices()
        {
            Vector3 cameraDir = _camera.LookDirection;
            Vector3 unitY = Vector3.UnitY;
            Vector3 cameraPosition = _camera.Position;
            FrustumCorners corners;
            FrustumHelpers.ComputePerspectiveFrustumCorners(
                ref cameraPosition,
                ref cameraDir,
                ref unitY,
                _camera.FieldOfViewRadians,
                _camera.NearPlaneDistance,
                _camera.FarPlaneDistance,
                (float)_rc.Window.Width / (float)_rc.Window.Height,
                out corners);

            // Approach used: http://alextardif.com/ShadowMapping.html

            Vector3 frustumCenter = Vector3.Zero;
            frustumCenter += corners.NearTopLeft;
            frustumCenter += corners.NearTopRight;
            frustumCenter += corners.NearBottomLeft;
            frustumCenter += corners.NearBottomRight;
            frustumCenter += corners.FarTopLeft;
            frustumCenter += corners.FarTopRight;
            frustumCenter += corners.FarBottomLeft;
            frustumCenter += corners.FarBottomRight;
            frustumCenter /= 8f;

            float radius = (corners.NearTopLeft - corners.FarBottomRight).Length() / 2.0f;
            float texelsPerUnit = (float)_shadowMapStage.DepthMapWidth / (radius * 2.0f);

            Matrix4x4 scalar = Matrix4x4.CreateScale(texelsPerUnit, texelsPerUnit, texelsPerUnit);

            Vector3 baseLookAt = -_lightDirection;

            Matrix4x4 lookat = Matrix4x4.CreateLookAt(Vector3.Zero, baseLookAt, Vector3.UnitY);
            lookat = scalar * lookat;
            Matrix4x4 lookatInv;
            Matrix4x4.Invert(lookat, out lookatInv);

            frustumCenter = Vector3.Transform(frustumCenter, lookat);
            frustumCenter.X = (int)frustumCenter.X;
            frustumCenter.Y = (int)frustumCenter.Y;
            frustumCenter = Vector3.Transform(frustumCenter, lookatInv);

            Vector3 eye = frustumCenter - (_lightDirection * radius * 2f);

            Matrix4x4 lightView = Matrix4x4.CreateLookAt(eye, frustumCenter, Vector3.UnitY);

            _lightProjMatrixProvider.Data = Matrix4x4.CreateOrthographicOffCenter(
                -radius, radius, -radius, radius, -radius * 4f, radius * 4f);
            _lightViewMatrixProvider.Data = lightView;
            _lightInfoProvider.Data = new Vector4(_lightDirection, 1);
        }

        private static void ToggleFullScreenState()
        {
            _rc.Window.WindowState = _rc.Window.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
        }

        private static void DrawMainMenu()
        {
            bool triggerPopup = false;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Scenes"))
                {

                    bool boxScene = _visibilityManager == _boxSceneVM;
                    if (boxScene)
                        ImGui.PushStyleColor(ColorTarget.Text, RgbaFloat.Cyan.ToVector4());
                    if (ImGui.MenuItem("Boxes", null))
                    {
                        _circleWidth = 12.0;
                        ChangeScene(SceneWithBoxes());
                    }
                    if (boxScene)
                        ImGui.PopStyleColor();

                    bool teapotScene = _visibilityManager == _teapotVM;
                    if (teapotScene)
                        ImGui.PushStyleColor(ColorTarget.Text, RgbaFloat.Cyan.ToVector4());
                    if (ImGui.MenuItem("Teapot", null))
                    {
                        _circleWidth = 5.0;
                        ChangeScene(SceneWithTeapot());
                    }
                    if (teapotScene)
                        ImGui.PopStyleColor();

                    bool shadowsScene = _visibilityManager == _shadowsScene;
                    if (shadowsScene)
                        ImGui.PushStyleColor(ColorTarget.Text, RgbaFloat.Cyan.ToVector4());
                    if (ImGui.MenuItem("Shadows", null))
                    {
                        _circleWidth = 15.0;
                        ChangeScene(SceneWithShadows());
                    }
                    if (shadowsScene)
                        ImGui.PopStyleColor();

                    bool roomScene = _visibilityManager == _octreeScene;
                    if (roomScene)
                        ImGui.PushStyleColor(ColorTarget.Text, RgbaFloat.Cyan.ToVector4());
                    if (ImGui.MenuItem("Octree", null))
                    {
                        _circleWidth = 8.0;
                        ChangeScene(OctreeScene());
                    }
                    if (roomScene)
                        ImGui.PopStyleColor();

                    bool sponzaAtriumScene = _visibilityManager == _sponzaAtrium;
                    if (sponzaAtriumScene)
                        ImGui.PushStyleColor(ColorTarget.Text, RgbaFloat.Cyan.ToVector4());
                    if (ImGui.MenuItem("Sponza Atrium", null))
                    {
                        _circleWidth = 8.0;
                        ChangeScene(SponzaAtriumScene());
                    }
                    if (sponzaAtriumScene)
                        ImGui.PopStyleColor();

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.MenuItem("Full Screen", "F11", _rc.Window.WindowState == WindowState.FullScreen, true))
                    {
                        ToggleFullScreenState();
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
                    if (ImGui.Checkbox("Limit Framerate", ref _limitFrameRate))
                    {
                        var threadedWindow = _rc.Window as DedicatedThreadWindow;
                        if (threadedWindow != null)
                        {
                            threadedWindow.LimitPollRate = _limitFrameRate;
                        }
                    }

                    ImGui.Checkbox("Auto-Rotate Light", ref _moveLight);

                    string apiName = (_rc is OpenGLRenderContext) ? "OpenGL" : "Direct3D";
                    if (ImGui.BeginMenu($"Renderer: {apiName}"))
                    {
                        foreach (var option in _backendOptions)
                        {
                            if (ImGui.MenuItem(option.Name, null))
                            {
                                if (_selectedOption != option)
                                {
                                    ChangeRenderContext(option.Create());
                                    _selectedOption = option;
                                }
                            }
                        }

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Camera Settings"))
                    {
                        bool perspectiveProjection = !_camera.UseOrthographicProjection;
                        string buttonLabel = perspectiveProjection ? "Perspective" : "Orthographic";
                        if (ImGui.Button(buttonLabel))
                        {
                            _camera.UseOrthographicProjection = perspectiveProjection;
                        }

                        if (perspectiveProjection)
                        {
                            float fov = _camera.FieldOfViewRadians;
                            if (ImGui.SliderFloat("FOV", ref fov, 0.05f, (float)Math.PI - .01f, fov.ToString(), 1f))
                            {
                                _camera.FieldOfViewRadians = fov;
                            }
                        }
                        else
                        {
                            float orthographicWidth = _camera.OrthographicWidth;
                            if (ImGui.DragFloat("Orthographic Width", ref orthographicWidth, 1f, 100f, 1f))
                            {
                                _camera.OrthographicWidth = orthographicWidth;
                            }
                        }

                        float nearDistance = _camera.NearPlaneDistance;
                        if (ImGui.DragFloat("Near Plane Distance", ref nearDistance, 5f, 1000f, .1f))
                        {
                            _camera.NearPlaneDistance = nearDistance;
                        }
                        float farDistance = _camera.FarPlaneDistance;
                        if (ImGui.DragFloat("Far Plane Distance", ref farDistance, 5f, 1000f, .1f))
                        {
                            _camera.FarPlaneDistance = farDistance;
                        }

                        ImGui.EndMenu();
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

                    if (ImGui.BeginMenu("Pipeline Stages"))
                    {
                        foreach (var stage in _configurableStages)
                        {
                            bool enabled = stage.Enabled;
                            if (ImGui.Checkbox(stage.Name, ref enabled))
                            {
                                stage.Enabled = !stage.Enabled;
                            }
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Edit Preferences", null))
                    {
                        _preferencesEditorOpened = true;
                    }
                    if (ImGui.MenuItem("Material Editor", null))
                    {
                        _editorWindow.Open();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("About"))
                {
                    if (ImGui.MenuItem("View Source", null))
                    {
                        string ghUrl = @"https://github.com/mellinoe/veldrid";
                        BrowserUtil.OpenBrowser(ghUrl);
                    }
                    if (ImGui.MenuItem("About Veldrid", null))
                    {
                        triggerPopup = true;
                    }
                    ImGui.EndMenu();
                }
                if (_rc.Window.WindowState == WindowState.FullScreen)
                {
                    ImGui.Text(string.Format("{0} FPS ({1} ms)", _fta.CurrentAverageFramesPerSecond.ToString("0.0"), _fta.CurrentAverageFrameTime.ToString("#00.00")));
                }

                ImGui.EndMainMenuBar();
            }

            if (triggerPopup)
            {
                ImGui.OpenPopup("AboutVeldridPopup");
            }

            if (ImGui.BeginPopup("AboutVeldridPopup"))
            {
                ImGui.Text(
@"Veldrid is an experimental renderer with Direct3D
and OpenGL backends, built with .NET Core.");
                ImGui.Text(
@"Source code is freely available at
https://github.com/mellinoe/veldrid.");
                ImGui.Text(
@"OpenGL bindings using OpenTK (https://github.com/opentk/opentk).");
                ImGui.Text(
@"Direct3D bindings using SharpDX (https://github.com/sharpdx/sharpdx).");
                ImGui.EndPopup();
            }

            DrawPreferencesEditor();
        }

        private static void ChangeScene(VisibiltyManager vm)
        {
            _visibilityManager = vm;
        }

        private static void DrawPreferencesEditor()
        {
            if (_preferencesEditorOpened)
            {
                ImGui.SetNextWindowSize(new Vector2(400, 300), SetCondition.FirstUseEver);
                if (ImGui.BeginWindow("Preferences Editor", ref _preferencesEditorOpened, WindowFlags.NoCollapse | WindowFlags.ShowBorders))
                {
                    object prefs = Preferences.Instance;
                    Drawer d = new ComplexItemDrawer(typeof(Preferences), false);
                    ImGui.SetNextTreeNodeOpened(true, SetCondition.FirstUseEver);
                    d.Draw("Preferences", ref prefs, _rc);
                    if (ImGui.Button("Save"))
                    {
                        Preferences.Instance.Save();
                    }
                }
                ImGui.EndWindow();
            }
        }

        private static void ChangeRenderContext(RenderContext newContext)
        {
            if (newContext != null)
            {
                CreatedResourceCache.ClearCache();

                foreach (var kvp in _rc.GetAllGlobalBufferProviderPairs())
                {
                    kvp.Value.Dispose();
                    newContext.RegisterGlobalDataProvider(kvp.Key, kvp.Value.DataProvider);
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
                        ((SwappableRenderItem)item).ChangeRenderContext(_ad, newContext);
                    }
                }
                if (_boxSceneVM != null)
                {
                    foreach (var item in _boxSceneVM.RenderItems)
                    {
                        ((SwappableRenderItem)item).ChangeRenderContext(_ad, newContext);
                    }
                }
                if (_shadowsScene != null)
                {
                    foreach (var item in _shadowsScene.RenderItems)
                    {
                        ((SwappableRenderItem)item).ChangeRenderContext(_ad, newContext);
                    }
                }
                if (_octreeScene != null)
                {
                    foreach (var item in _octreeScene.RenderItems)
                    {
                        ((SwappableRenderItem)item).ChangeRenderContext(_ad, newContext);
                    }
                }
                if (_sponzaAtrium != null)
                {
                    foreach (var item in _sponzaAtrium.RenderItems)
                    {
                        ((SwappableRenderItem)item).ChangeRenderContext(_ad, newContext);
                    }
                }

                _rc.Dispose();
                _rc = newContext;

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

            BoundingFrustum frustum = new BoundingFrustum(_camera.ViewProvider.Data * _camera.ProjectionProvider.Data);
            ((StandardPipelineStage)_renderer.Stages[1]).CameraFrustum = frustum;

            _renderer.RenderFrame(_visibilityManager, _camera.Position);
            _imguiRenderer.NewFrame();

            if (_takeScreenshot)
            {
                _takeScreenshot = false;
                _rc.SetDefaultFramebuffer();
                int width = _rc.Window.Width;
                int height = _rc.Window.Height;
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

        public class RendererOption
        {
            public string Name { get; }
            public Func<RenderContext> Create;

            public RendererOption(string name, Func<RenderContext> create)
            {
                Name = name;
                Create = create;
            }
        }
    }
}
