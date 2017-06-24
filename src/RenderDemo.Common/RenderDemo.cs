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
using Veldrid.Graphics.OpenGLES;

namespace Veldrid.RenderDemo
{
    public static class RenderDemo
    {
        private static Renderer _renderer;
        private static RenderContext _rc;
        private static Window _window;
        private static ShadowMapStage _shadowMapStage;
        private static RendererOption[] _backendOptions;
        private static FrameTimeAverager _fta;
        private static double _desiredFrameLengthSeconds = 1.0 / 60.0;
        private static bool _limitFrameRate = true;
        private static VisibiltyManager _visibilityManager;
        private static ConstantDataProvider<DirectionalLightBuffer> _lightBufferProvider;
        private static DynamicDataProvider<Matrix4x4> _lightViewMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Matrix4x4> _lightProjMatrixProvider = new DynamicDataProvider<Matrix4x4>();
        private static DynamicDataProvider<Vector4> _lightInfoProvider = new DynamicDataProvider<Vector4>();
        private static DynamicDataProvider<PointLightsBuffer> _pointLightsProvider = new DynamicDataProvider<PointLightsBuffer>();
        private static FlatListVisibilityManager _boxSceneVM;
        private static FlatListVisibilityManager _teapotVM;
        private static OctreeVisibilityManager _shadowsScene;
        private static FlatListVisibilityManager _octreeScene;
        private static OctreeVisibilityManager _sponzaAtrium;
        private static FlatListVisibilityManager _instancingScene;
        private static FlatListVisibilityManager _geometryShaderScene;
        private static double _circleWidth = 12.0;
        private static bool _wireframe;

        private static bool _takeScreenshot;
        private static SwappableImGuiRenderer _imguiRenderer;

        private static Framebuffer _screenshotFramebuffer;
        private static RasterizerState _wireframeRasterizerState;
        private static MaterialAsset s_mtlMaterialAsset;

        private static Camera _camera;
        private static float _cameraYaw;
        private static float _cameraPitch;

        private static Vector3 _lightDirection;
        private static bool _autoRotateCamera = false;
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
        private static Octree<ShadowCaster> _octree;
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
        private static List<RayCastHit<ShadowCaster>> _octreeRayCastResult = new List<RayCastHit<ShadowCaster>>();
        private static List<float> _rayCastDistances = new List<float>();
        private static List<RenderItem> _sponzaQueryResult = new List<RenderItem>();
        private static BoundingBoxWireframeRenderer _sceneBoundsRenderer;
        private static bool s_needsResizing;

        public static void RunDemo(RenderContext renderContext, Window window, params RendererOption[] backendOptions)
        {
            try
            {
                _rc = renderContext;
                _window = window;
                _backendOptions = backendOptions;
                _selectedOption = backendOptions.FirstOrDefault();

                _window.Resized += OnWindowResized;

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

                _imguiRenderer = new SwappableImGuiRenderer(_rc, window);
                _imguiRenderer.SetRenderStages(CommonStages.ImGui);

                _lightBufferProvider = new ConstantDataProvider<DirectionalLightBuffer>(
                    new DirectionalLightBuffer(RgbaFloat.White, new Vector3(-.3f, -1f, -1f)));

                // Shader buffers for shadow mapping
                _lightDirection = Vector3.Normalize(new Vector3(0f, -1f, -1f));
                _lightInfoProvider.Data = new Vector4(_lightDirection, 1);

                _camera = new Camera(window);
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
                _rc.RegisterGlobalDataProvider("PointLights", _pointLightsProvider);
                _rc.ClearColor = RgbaFloat.CornflowerBlue;

                UpdateLightMatrices();
                UpdatePointLights();

                CreateScreenshotFramebuffer();
                CreateWireframeRasterizerState();

                ChangeScene(SceneWithBoxes());

                _fta = new FrameTimeAverager(0.666);

                long previousFrameTicks = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (window.Exists)
                {
                    long currentFrameTicks = sw.ElapsedTicks;
                    double deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

                    while (_limitFrameRate && deltaSeconds < _desiredFrameLengthSeconds)
                    {
                        currentFrameTicks = sw.ElapsedTicks;
                        deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                    }

                    previousFrameTicks = currentFrameTicks;

                    var snapshot = window.GetInputSnapshot();
                    InputTracker.UpdateFrameInput(snapshot);
                    Update(deltaSeconds, snapshot);
                    Draw();
                }

            }
            catch when (!Debugger.IsAttached)
            {
                if (_rc is OpenGLRenderContext)
                {
                    // Console.WriteLine("GL Error: " + GL.GetError());
                }
            }
        }

        private static void OnWindowResized()
        {
            s_needsResizing = true;
        }

        private static void UpdatePointLights()
        {
            var buffer = _pointLightsProvider.Data;
            buffer.LightInfo0 = new PointLightInfo(_camera.Position, new Vector3(1, 1, 1), 50f);
            buffer.LightInfo1 = new PointLightInfo(new Vector3(55, 5, 6), new Vector3(1, 1, 1), 65);
            buffer.LightInfo2 = new PointLightInfo(new Vector3(-100, 8, 9), new Vector3(1, 1, 1), 75);
            buffer.LightInfo3 = new PointLightInfo(new Vector3(0, 10, 0), new Vector3(.89f, .35f, .13f), 45);
            buffer.NumActivePointLights = 4;

            _pointLightsProvider.Data = buffer;
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

        private static FlatListVisibilityManager SceneWithInstancing()
        {
            if (_instancingScene == null)
            {
                _instancingScene = new FlatListVisibilityManager();
                InstancedSphereRenderer isr = new InstancedSphereRenderer(_ad, _rc);
                _instancingScene.AddRenderItem(isr);

                var plane = new TexturedMeshRenderer(_ad, _rc, PlaneModel.Vertices, PlaneModel.Indices, Textures.WoodTexture);
                plane.Position = new Vector3(0, -2, 0);
                plane.Scale = new Vector3(20, 1, 20);
                _instancingScene.AddRenderItem(plane);

                _instancingScene.AddRenderItem(_imguiRenderer);
            }

            return _instancingScene;
        }

        private static FlatListVisibilityManager GeometryShaderScene()
        {
            if (_geometryShaderScene == null)
            {
                _geometryShaderScene = new FlatListVisibilityManager();
                _geometryShaderScene.AddRenderItem(_imguiRenderer);

                GeometryShaderBox gsb = new GeometryShaderBox(_ad, _rc, _camera);
                _geometryShaderScene.AddRenderItem(gsb);

                GeometryShaderBox gsb2 = new GeometryShaderBox(_ad, _rc, _camera, "billboard-geometry");
                _geometryShaderScene.AddRenderItem(gsb2);
                gsb2.Position = new Vector3(7.5f, -2.5f, 7.5f);

                var sphere = _ad.LoadAsset<ObjFile>(new AssetID("Models/Sphere.obj")).GetFirstMesh();
                var tcr = new TexturedMeshRenderer(_ad, _rc, sphere.Vertices, sphere.Indices, Textures.CubeTexture);
                tcr.Position = new Vector3(-5f, 0, 0);
                _geometryShaderScene.AddRenderItem(tcr);

                var tcr2 = new TexturedMeshRenderer(_ad, _rc, sphere.Vertices, sphere.Indices, Textures.CubeTexture);
                tcr2.Position = new Vector3(5f, 0, 0);
                _geometryShaderScene.AddRenderItem(tcr2);

                var plane = new TexturedMeshRenderer(_ad, _rc, PlaneModel.Vertices, PlaneModel.Indices, Textures.WoodTexture);
                plane.Position = new Vector3(0, -5, 0);
                plane.Scale = new Vector3(20, 1, 20);
                _geometryShaderScene.AddRenderItem(plane);
            }

            return _geometryShaderScene;
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

                var cube3 = new ShadowCaster(_rc, _ad, CubeModel.Vertices, CubeModel.Indices, stoneMaterial);
                cube3.Position = new Vector3(-4f, 0f, 6f);
                cube3.Rotation = Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateRotationY(1));
                _shadowsScene.AddRenderItem(cube3.BoundingBox, cube3);

                var sphereModel = _ad.LoadAsset<ObjFile>(new AssetID("Models/Sphere.obj")).GetFirstMesh();
                var sphere = new ShadowCaster(_rc, _ad, sphereModel.Vertices, sphereModel.Indices, stoneMaterial);
                sphere.Position = new Vector3(18f, 3f, -18f);
                sphere.Scale = new Vector3(4.0f);
                sphere.Rotation = Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateRotationY(1));
                _shadowsScene.AddRenderItem(sphere.BoundingBox, sphere);


                var plane = new ShadowCaster(_rc, _ad, PlaneModel.Vertices, PlaneModel.Indices, woodMaterial);
                plane.Position = new Vector3(0, -2.5f, 0);
                plane.Scale = new Vector3(50f);
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
                _octree = new Octree<ShadowCaster>(bounds, 3);
                _octreeRenderer = new OctreeRenderer<ShadowCaster>(_octree.CurrentRoot, _ad, _rc);
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
            var node = _octree.CurrentRoot;
            int removed = 0;

            Random r = new Random();
            var allItems = node.GetAllOctreeItems().ToArray();
            foreach (var octreeItem in allItems)
            {
                Vector3 offset = new Vector3((float)(r.NextDouble()) * 15.0f, (float)(r.NextDouble() - 0.5f) * 5.0f, (float)(r.NextDouble() - 0.5f) * 5.0f);
                _octree.MoveItem(octreeItem, new BoundingBox(octreeItem.Bounds.Min + offset, octreeItem.Bounds.Max + offset));
                octreeItem.Item.Position += offset;
                removed++;
                if (removed == 4)
                {
                    //break;
                }
            }

            // Root node may have changed.
            _octreeRenderer.Octree = _octree.CurrentRoot;
        }

        private static void AddOctreeCube(Vector3 position)
        {
            var cube = new ShadowCaster(_rc, _ad, CubeModel.Vertices, CubeModel.Indices, _stoneMaterial);
            cube.Position = position;
            _octreeScene.AddRenderItem(cube);
            _octree.AddItem(cube.BoundingBox, cube);
            _octreeRenderer.Octree = _octree.CurrentRoot;
            _octreeCubes.Add(cube);
        }

        private static OctreeVisibilityManager SponzaAtriumScene()
        {
            if (_sponzaAtrium == null)
            {
                Stopwatch sw = Stopwatch.StartNew();
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
                        overrideTextureData = _ad.LoadAsset<ImageSharpTexture>(texturePath);
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

                sw.Stop();
                Console.WriteLine("Total elapsed loading time: " + sw.Elapsed);
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

        private static void Update(double deltaSeconds, InputSnapshot snapshot)
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

            _imguiRenderer.Update(_window, (float)deltaSeconds);
            _imguiRenderer.OnInputUpdated(_window, snapshot);
            DrawMainMenu();

            _fta.AddTime(deltaSeconds);
            string apiName = (_rc is OpenGLRenderContext) ? "OpenGL" : (_rc is OpenGLESRenderContext) ? "OpenGL ES" : "Direct3D";
            _window.Title = $"[{apiName}] " + _fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms");
            if (InputTracker.GetKeyDown(Key.F4) && (InputTracker.GetKey(Key.AltLeft) || InputTracker.GetKey(Key.AltRight)))
            {
                _window.Close();
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
                    int numHits = _octree.RayCast(r, _octreeRayCastResult, OctreeFilter);

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

            }
            if (_visibilityManager == _shadowsScene || _visibilityManager == _sponzaAtrium)
            {
                _sceneBoundsRenderer.Box = ((OctreeVisibilityManager)_visibilityManager).OctreeRootNode.GetPreciseBounds();
            }
            UpdateLightMatrices();
            UpdatePointLights();

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

            float deltaSecondsFloat = (float)deltaSeconds;

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
                        _camera.Position += _camera.LookDirection * _cameraMoveSpeed * sprintFactor * deltaSecondsFloat;
                        if (_camera.UseOrthographicProjection)
                        {
                            _camera.OrthographicWidth -= 5f * deltaSecondsFloat * sprintFactor;
                        }
                    }
                    if (InputTracker.GetKey(Key.S))
                    {
                        _camera.Position -= _camera.LookDirection * _cameraMoveSpeed * sprintFactor * deltaSecondsFloat;
                        if (_camera.UseOrthographicProjection)
                        {
                            _camera.OrthographicWidth += 5f * deltaSecondsFloat * sprintFactor;
                        }
                    }
                    if (InputTracker.GetKey(Key.D))
                    {
                        _camera.Position += cameraRight * _cameraMoveSpeed * sprintFactor * deltaSecondsFloat;
                    }
                    if (InputTracker.GetKey(Key.A))
                    {
                        _camera.Position -= cameraRight * _cameraMoveSpeed * sprintFactor * deltaSecondsFloat;
                    }
                    if (InputTracker.GetKey(Key.E))
                    {
                        _camera.Position += cameraUp * _cameraMoveSpeed * sprintFactor * deltaSecondsFloat;
                    }
                    if (InputTracker.GetKey(Key.Q))
                    {
                        _camera.Position -= cameraUp * _cameraMoveSpeed * sprintFactor * deltaSecondsFloat;
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
        }

        private static int OctreeFilter(Ray ray, ShadowCaster item, List<RayCastHit<ShadowCaster>> hits)
        {
            int result = item.RayCast(ray, _rayCastDistances);
            for (int i = 0; i < result; i++)
            {
                float distance = _rayCastDistances[_rayCastDistances.Count - i - 1];
                hits.Add(new RayCastHit<ShadowCaster>(item, ray.Origin + ray.Direction * distance, distance));
            }

            return result;
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
                (float)_window.Width / (float)_window.Height,
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

            Vector3 lightPos = frustumCenter - (_lightDirection * radius * 2f);

            Matrix4x4 lightView = Matrix4x4.CreateLookAt(lightPos, frustumCenter, Vector3.UnitY);

            _lightProjMatrixProvider.Data = Matrix4x4.CreateOrthographicOffCenter(
                -radius, radius, -radius, radius, -radius * 4f, radius * 4f);
            _lightViewMatrixProvider.Data = lightView;
            _lightInfoProvider.Data = new Vector4(_lightDirection, 1);
            _shadowMapStage.DirectionalLightPosition = lightPos;
        }

        private static void ToggleFullScreenState()
        {
            _window.WindowState = _window.WindowState == WindowState.BorderlessFullScreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
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

                    bool instancingScene = _visibilityManager == _instancingScene;
                    if (instancingScene)
                        ImGui.PushStyleColor(ColorTarget.Text, RgbaFloat.Cyan.ToVector4());
                    if (ImGui.MenuItem("Instancing", null))
                    {
                        _circleWidth = 15.0;
                        ChangeScene(SceneWithInstancing());
                    }
                    if (instancingScene)
                        ImGui.PopStyleColor();

                    bool geometryScene = _visibilityManager == _geometryShaderScene;
                    if (geometryScene)
                        ImGui.PushStyleColor(ColorTarget.Text, RgbaFloat.Cyan.ToVector4());
                    if (ImGui.MenuItem("Geometry Shader", null))
                    {
                        _circleWidth = 15.0;
                        ChangeScene(GeometryShaderScene());
                    }
                    if (geometryScene)
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
                    if (ImGui.MenuItem("Full Screen", "F11", _window.WindowState == WindowState.BorderlessFullScreen, true))
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
                        var threadedWindow = _window as Sdl2.Sdl2Window;
                        if (threadedWindow != null)
                        {
                            threadedWindow.LimitPollRate = _limitFrameRate;
                        }
                    }

                    ImGui.Checkbox("Auto-Rotate Light", ref _moveLight);

                    string apiName = (_rc is OpenGLRenderContext) ? "OpenGL" : (_rc is OpenGLESRenderContext) ? "OpenGL ES" : "Direct3D";
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
                if (_window.WindowState == WindowState.BorderlessFullScreen)
                {
                    ImGui.Text(string.Format("{0} FPS ({1} ms)", _fta.CurrentAverageFramesPerSecond.ToString("0.0"), _fta.CurrentAverageFrameTimeSeconds.ToString("#00.00")));
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
                    ImGui.SetNextTreeNodeOpen(true, SetCondition.FirstUseEver);
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
                ImGuiImageHelper.InvalidateCache();

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
                if (_instancingScene != null)
                {
                    foreach (var item in _instancingScene.RenderItems)
                    {
                        ((SwappableRenderItem)item).ChangeRenderContext(_ad, newContext);
                    }
                }
                if (_geometryShaderScene != null)
                {
                    foreach (var item in _geometryShaderScene.RenderItems)
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

            if (s_needsResizing)
            {
                s_needsResizing = false;
                _rc.NotifyWindowResized(_window.Width, _window.Height);
            }
            _renderer.RenderFrame(_visibilityManager, _camera.Position);

            if (_takeScreenshot)
            {
                _takeScreenshot = false;
                _rc.SetDefaultFramebuffer();
                int width = _window.Width;
                int height = _window.Height;
                var cpuDepthTexture = new RawTextureDataArray<ushort>(width, height, sizeof(ushort), Graphics.PixelFormat.R16_UInt);
                _screenshotFramebuffer.DepthTexture.CopyTo(cpuDepthTexture);

                ImageSharp.Image<ImageSharp.Rgba32>  image = new ImageSharp.Image<ImageSharp.Rgba32>(width, height);
                unsafe
                {
                    fixed (ImageSharp.Rgba32* pixelsPtr = &image.Pixels.DangerousGetPinnableReference())
                    {
                        PixelFormatConversion.ConvertPixelsUInt16DepthToRgbaFloat(width * height, cpuDepthTexture.PixelData, pixelsPtr);
                    }
                }
                ImageSharpTexture rgbaDepthTexture = new ImageSharpTexture(image);
                Console.WriteLine($"Saving file: {width} x {height}, ratio:{(double)width / height}");
                rgbaDepthTexture.SaveToFile(Environment.TickCount + ".png");
            }
        }

        private static TextureData LoadStoneTextureData()
        {
            return new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Textures/CubeTexture.png"));
        }

        private class FrameTimeAverager
        {
            private readonly double _timeLimit = 666;

            private double _accumulatedTime = 0;
            private int _frameCount = 0;
            private readonly double _decayRate = .3;

            public double CurrentAverageFrameTimeSeconds { get; private set; }
            public double CurrentAverageFrameTimeMilliseconds => CurrentAverageFrameTimeSeconds * 1000.0;
            public double CurrentAverageFramesPerSecond => 1 / CurrentAverageFrameTimeSeconds;

            public FrameTimeAverager(double maxTimeSeconds)
            {
                _timeLimit = maxTimeSeconds;
            }

            public void Reset()
            {
                _accumulatedTime = 0;
                _frameCount = 0;
            }

            public void AddTime(double seconds)
            {
                _accumulatedTime += seconds;
                _frameCount++;
                if (_accumulatedTime >= _timeLimit)
                {
                    Average();
                }
            }

            private void Average()
            {
                double total = _accumulatedTime;
                CurrentAverageFrameTimeSeconds =
                    (CurrentAverageFrameTimeSeconds * _decayRate)
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
