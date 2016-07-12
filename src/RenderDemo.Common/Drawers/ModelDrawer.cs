using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Assets;
using Veldrid.Graphics;
using Veldrid.Graphics.Pipeline;
using Veldrid.RenderDemo.ForwardRendering;
using Veldrid.RenderDemo.Models;

namespace Veldrid.RenderDemo.Drawers
{
    public class ModelDrawer : Drawer<ConstructedMeshInfo>
    {
        private static ConditionalWeakTable<ConstructedMeshInfo, PreviewScene> _previewScenes = new ConditionalWeakTable<ConstructedMeshInfo, PreviewScene>();
        private static RenderContext s_validContext;

        public override bool Draw(string label, ref ConstructedMeshInfo obj, RenderContext rc)
        {
            Vector2 region = ImGui.GetContentRegionAvailable();
            float minDimension = Math.Min(900, Math.Min(region.X, region.Y)) - 50;
            Vector2 imageDimensions = new Vector2(minDimension, minDimension / (1.33f));

            PreviewScene scene;
            scene = GetOrCreateScene(obj, rc);
            scene.Size = new Size((int)imageDimensions.X, (int)imageDimensions.Y);
            scene.RenderFrame();
            IntPtr id = ImGuiImageHelper.GetOrCreateImGuiBinding(rc, scene.RenderedScene);
            ImGui.Image(id, new Vector2(scene.Width, scene.Height), rc.TopLeftUv, rc.BottomRightUv, Vector4.One, Vector4.One);

            return false;
        }

        private static PreviewScene GetOrCreateScene(ConstructedMeshInfo obj, RenderContext rc)
        {
            if (s_validContext != rc)
            {
                s_validContext = rc;
                _previewScenes = new ConditionalWeakTable<ConstructedMeshInfo, PreviewScene>();
            }

            PreviewScene scene;
            if (!_previewScenes.TryGetValue(obj, out scene))
            {
                scene = new PreviewScene(rc, obj);
                _previewScenes.Add(obj, scene);
            }

            return scene;
        }

        private class PreviewScene
        {
            private Size _size = new Size(500, 360);
            public Size Size { get { return _size; } set { if (_size != value) { _size = value; OnSizeChanged(); } } }

            public int Width => _size.Width;
            public int Height => _size.Height;

            public float Fov { get; set; } = 1.05f;

            public bool AutoRotateCamera { get; set; } = true;

            RenderContext _rc;
            private Framebuffer _fb;

            private readonly PreviewModel _previewItem;
            private readonly PreviewModel _floor;

            private readonly DynamicDataProvider<Matrix4x4> _projection;
            private readonly DynamicDataProvider<Matrix4x4> _view;

            private readonly DynamicDataProvider<Matrix4x4> _lightProjection;
            private readonly DynamicDataProvider<Matrix4x4> _lightView;
            private readonly DynamicDataProvider<Vector4> _lightInfo;

            private readonly Dictionary<string, ConstantBufferDataProvider> _sceneProviders = new Dictionary<string, ConstantBufferDataProvider>();

            private readonly PipelineStage[] _stages;
            private readonly FlatListVisibilityManager _visiblityManager;
            private Vector3 _lightDirection = Vector3.Normalize(new Vector3(-1f, -.6f, -.3f));
            private Vector3 _cameraPosition;
            private double _circleWidth = 10.0f;
            private readonly StandardPipelineStage _standardPipelineStage;

            public PreviewScene(RenderContext rc, ConstructedMeshInfo previewItem)
            {
                _rc = rc;
                ResourceFactory factory = rc.ResourceFactory;
                _fb = factory.CreateFramebuffer(Width, Height);

                _projection = new DynamicDataProvider<Matrix4x4>();
                UpdateProjectionData();
                _view = new DynamicDataProvider<Matrix4x4>(Matrix4x4.CreateLookAt(Vector3.UnitZ * 7f + Vector3.UnitY * 1.5f, Vector3.Zero, Vector3.UnitY));

                _lightProjection = new DynamicDataProvider<Matrix4x4>(Matrix4x4.CreateOrthographicOffCenter(-18, 18, -18, 18, -10, 60f));
                _lightView = new DynamicDataProvider<Matrix4x4>(Matrix4x4.CreateLookAt(-_lightDirection * 20f, Vector3.Zero, Vector3.UnitY));
                _lightInfo = new DynamicDataProvider<Vector4>(new Vector4(_lightDirection, 1));

                _standardPipelineStage = new StandardPipelineStage(rc, "Standard", _fb);
                _stages = new PipelineStage[]
                {
                    new ShadowMapStage(rc, "ShadowMap_Preview"),
                    _standardPipelineStage,
                };

                _sceneProviders.Add("ProjectionMatrix", _projection);
                _sceneProviders.Add("ViewMatrix", _view);
                _sceneProviders.Add("LightProjMatrix", _lightProjection);
                _sceneProviders.Add("LightViewMatrix", _lightView);
                _sceneProviders.Add("LightInfo", _lightInfo);

                _floor = CreatePreviewModel(PlaneModel.Vertices, PlaneModel.Indices);
                _floor.WorldMatrix.Data = Matrix4x4.CreateScale(10f, 1f, 10f);

                _previewItem = CreatePreviewModel(previewItem.Vertices, previewItem.Indices);
                _previewItem.WorldMatrix.Data = Matrix4x4.CreateTranslation(0, 1.5f, 0);

                _visiblityManager = new FlatListVisibilityManager();
                _visiblityManager.AddRenderItem(_floor);
                _visiblityManager.AddRenderItem(_previewItem);
            }

            public DeviceTexture RenderedScene => _fb.ColorTexture;

            private PreviewModel CreatePreviewModel(VertexPositionNormalTexture[] vertices, int[] indices, ShaderTextureBinding textureBinding = null)
            {
                AssetDatabase lfd = new LooseFileDatabase(Path.Combine(AppContext.BaseDirectory, "Assets"));
                VertexBuffer vb = _rc.ResourceFactory.CreateVertexBuffer(vertices.Length * VertexPositionNormalTexture.SizeInBytes, false);
                vb.SetVertexData(
                    vertices,
                    new VertexDescriptor(VertexPositionNormalTexture.SizeInBytes, VertexPositionNormalTexture.ElementCount, 0, IntPtr.Zero));

                IndexBuffer ib = _rc.ResourceFactory.CreateIndexBuffer(indices.Length * sizeof(int), false);
                ib.SetIndices(indices, sizeof(int), 0);

                MaterialAsset shadowmapAsset = lfd.LoadAsset<MaterialAsset>("MaterialAsset/ShadowCaster_ShadowMap.json");
                MaterialAsset surfaceMaterial = lfd.LoadAsset<MaterialAsset>("MaterialAsset/ModelPreview.json");
                Material shadowmapMaterial = shadowmapAsset.Create(lfd, _rc, _sceneProviders);
                Material regularMaterial = surfaceMaterial.Create(lfd, _rc, _sceneProviders);

                return new PreviewModel(
                    vb,
                    ib,
                    indices.Length,
                    regularMaterial,
                    shadowmapMaterial,
                    new DynamicDataProvider<Matrix4x4>(Matrix4x4.Identity),
                    textureBinding);
            }

            public void RenderFrame()
            {
                UpdateCamera();

                _rc.SetFramebuffer(_fb);
                _rc.ClearBuffer(RgbaFloat.Clear);
                foreach (var stage in _stages)
                {
                    stage.ExecuteStage(_visiblityManager);
                }
            }

            private void UpdateCamera()
            {
                float timeFactor = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
                if (AutoRotateCamera)
                {
                    _cameraPosition = new Vector3(
                        (float)(Math.Cos(timeFactor) * _circleWidth),
                        6 + (float)Math.Sin(timeFactor) * 2,
                        (float)(Math.Sin(timeFactor) * _circleWidth));
                    _view.Data = Matrix4x4.CreateLookAt(_cameraPosition, -_cameraPosition, Vector3.UnitY);
                }
            }

            private void UpdateProjectionData()
            {
                _projection.Data = Matrix4x4.CreatePerspectiveFieldOfView(Fov, (float)Width / Height, 0.1f, 100f);
            }

            private void OnSizeChanged()
            {
                _fb.Dispose();
                _fb = _rc.ResourceFactory.CreateFramebuffer(Width, Height);
                _standardPipelineStage.OverrideFramebuffer = _fb;
                UpdateProjectionData();
            }
        }

        private class PreviewModel : RenderItem
        {
            private readonly VertexBuffer _vb;
            private readonly IndexBuffer _ib;
            private readonly int _elementCount;
            private readonly Material _shadowmapMaterial;
            private readonly Material _regularMaterial;
            private readonly DynamicDataProvider<Matrix4x4> _worldProvider;
            private readonly ConstantBufferDataProvider _inverseWorldProvider;
            private readonly ConstantBufferDataProvider[] _perObjectInputs;
            private readonly ShaderTextureBinding _textureBinding;

            private static readonly string[] s_stages = new string[] { "ShadowMap", "Standard" };

            public DynamicDataProvider<Matrix4x4> WorldMatrix => _worldProvider;

            public PreviewModel(
                VertexBuffer vb,
                IndexBuffer ib,
                int elementCount,
                Material regularMaterial,
                Material shadowmapMaterial,
                DynamicDataProvider<Matrix4x4> worldProvider,
                ShaderTextureBinding surfaceTextureBinding = null)
            {
                _vb = vb;
                _ib = ib;
                _elementCount = elementCount;
                _regularMaterial = regularMaterial;
                _shadowmapMaterial = shadowmapMaterial;
                _worldProvider = worldProvider;
                _inverseWorldProvider = new DependantDataProvider<Matrix4x4>(worldProvider, Utilities.CalculateInverseTranspose);
                _perObjectInputs = new ConstantBufferDataProvider[] { _worldProvider, _inverseWorldProvider };
                _textureBinding = surfaceTextureBinding;
            }

            public void Render(RenderContext rc, string stage)
            {
                rc.SetVertexBuffer(_vb);
                rc.SetIndexBuffer(_ib);
                if (stage == "ShadowMap")
                {
                    rc.SetMaterial(_shadowmapMaterial);
                    _shadowmapMaterial.ApplyPerObjectInput(_perObjectInputs[0]);
                }
                else
                {
                    rc.SetMaterial(_regularMaterial);
                    _regularMaterial.ApplyPerObjectInputs(_perObjectInputs);
                    if (_textureBinding != null)
                    {
                        _regularMaterial.UseTexture(1, _textureBinding);
                    }
                }

                rc.DrawIndexedPrimitives(_elementCount, 0);
            }

            public IEnumerable<string> GetStagesParticipated()
            {
                return s_stages;
            }

            public RenderOrderKey GetRenderOrderKey()
            {
                return new RenderOrderKey();
            }

            public bool Cull(ref BoundingFrustum visibleFrustum)
            {
                return false;
            }
        }
    }
}
