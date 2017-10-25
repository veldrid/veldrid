using ShaderGen;
using System.Numerics;
using System.Runtime.CompilerServices;
using System;
using Vd2.ImageSharp;
using Vd2.Utilities;
using System.Collections.Generic;

namespace Vd2.NeoDemo.Objects
{
    public class TexturedMesh : CullRenderable
    {
        private readonly MeshData _meshData;
        private readonly ImageSharpTexture _textureData;
        private readonly Transform _transform = new Transform();

        private BoundingBox _centeredBounds;
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private int _indexCount;
        private Texture2D _texture;
        private TextureView _textureView;
        private Texture2D _alphamapTexture;
        private TextureView _alphaMapView;

        private Pipeline _pipeline;
        private ResourceLayout _mainLayout;
        private ResourceSet _resourceSet;

        private Pipeline _shadowMapPipeline;
        private ResourceSet[] _shadowMapResourceSets;

        private UniformBuffer _worldBuffer;
        private UniformBuffer _inverseTransposeWorldBuffer;

        private readonly DisposeCollector _disposeCollector = new DisposeCollector();

        private readonly MaterialPropsAndBuffer _materialProps;

        private bool _materialPropsOwned = false;
        private ResourceLayout _depthLayout;

        public MaterialProperties MaterialProperties { get => _materialProps.Properties; set { _materialProps.Properties = value; } }

        public Transform Transform => _transform;

        public TexturedMesh(MeshData meshData, ImageSharpTexture textureData, MaterialProperties materialProps)
            : this(meshData, textureData, new MaterialPropsAndBuffer(materialProps))
        {
            _materialPropsOwned = true;
        }

        public TexturedMesh(MeshData meshData, ImageSharpTexture textureData, MaterialPropsAndBuffer materialProps)
        {
            _meshData = meshData;
            _centeredBounds = meshData.GetBoundingBox();
            _textureData = textureData;
            MaterialProperties defaultProps = new MaterialProperties { SpecularIntensity = new Vector3(0.3f), SpecularPower = 10f };
            _materialProps = materialProps;
        }

        public override BoundingBox BoundingBox => BoundingBox.Transform(_centeredBounds, _transform.GetTransformMatrix());

        public unsafe override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;
            _vb = _meshData.CreateVertexBuffer(factory, cl);
            _ib = _meshData.CreateIndexBuffer(factory, cl, out _indexCount);

            _worldBuffer = factory.CreateUniformBuffer(new BufferDescription(64, true));
            _inverseTransposeWorldBuffer = factory.CreateUniformBuffer(new BufferDescription(64, true));
            if (_materialPropsOwned)
            {
                _materialProps.CreateDeviceObjects(gd, cl, sc);
            }

            _disposeCollector.Add(_vb, _ib, _worldBuffer, _inverseTransposeWorldBuffer);

            _texture = _textureData.CreateDeviceTexture(factory, cl);
            _textureView = factory.CreateTextureView(_texture);
            _alphamapTexture = factory.CreateTexture2D(new TextureDescription(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            RgbaByte color = RgbaByte.White;
            cl.UpdateTexture2D(_alphamapTexture, (IntPtr)(&color), 4, 0, 0, 1, 1, 0, 0);
            _alphaMapView = factory.CreateTextureView(_alphamapTexture);

            _disposeCollector.Add(_texture, _textureView, _alphamapTexture, _alphaMapView);

            VertexLayoutDescription[] shadowDepthVertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            };

            Shader depthVS = ShaderHelper.LoadShader(factory, "ShadowDepth", ShaderStages.Vertex);
            Shader depthFS = ShaderHelper.LoadShader(factory, "ShadowDepth", ShaderStages.Fragment);
            ShaderStageDescription[] shadowDepthShaderStages = new ShaderStageDescription[]
            {
                new ShaderStageDescription(ShaderStages.Vertex, depthVS, "VS"),
                new ShaderStageDescription(ShaderStages.Fragment, depthFS, "FS"),
            };

            _disposeCollector.Add(depthVS, depthFS);

            _depthLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
             new ResourceLayoutElementDescription("ViewProjection", ResourceKind.Uniform, ShaderStages.Vertex),
             new ResourceLayoutElementDescription("World", ResourceKind.Uniform, ShaderStages.Vertex)));

            PipelineDescription depthPD = new PipelineDescription(
                BlendStateDescription.Empty,
                DepthStencilStateDescription.LessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(shadowDepthVertexLayouts, shadowDepthShaderStages),
                _depthLayout,
                DemoOutputsDescriptions.ShadowMapPass);
            _shadowMapPipeline = factory.CreatePipeline(ref depthPD);

            _shadowMapResourceSets = CreateShadowMapResourceSets(factory, cl, sc);

            _disposeCollector.Add(_depthLayout, _shadowMapPipeline);
            _disposeCollector.Add(_shadowMapResourceSets);

            VertexLayoutDescription[] mainVertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            };

            Shader mainVS = ShaderHelper.LoadShader(factory, "ShadowMain", ShaderStages.Vertex);
            Shader mainFS = ShaderHelper.LoadShader(factory, "ShadowMain", ShaderStages.Fragment);
            ShaderStageDescription[] mainShaderStages = new ShaderStageDescription[]
            {
                new ShaderStageDescription(ShaderStages.Vertex, mainVS, "VS"),
                new ShaderStageDescription(ShaderStages.Fragment, mainFS, "FS"),
            };
            _disposeCollector.Add(mainVS, mainFS);

            _mainLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("View", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("World", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("InverseTransposeWorld", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightViewProjection1", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightViewProjection2", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightViewProjection3", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("DepthLimits", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightInfo", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("PointLights", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("MaterialProperties", ResourceKind.Uniform, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.Texture, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("RegularSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AlphaMap", ResourceKind.Texture, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AlphaMapSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapNear", ResourceKind.Texture, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapMid", ResourceKind.Texture, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapFar", ResourceKind.Texture, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            PipelineDescription mainPD = new PipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.LessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(mainVertexLayouts, mainShaderStages),
                _mainLayout,
                gd.SwapchainFramebuffer.OutputDescription);
            _pipeline = factory.CreatePipeline(ref mainPD);

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(_mainLayout,
                sc.ProjectionMatrixBuffer,
                sc.ViewMatrixBuffer,
                _worldBuffer,
                _inverseTransposeWorldBuffer,
                sc.LightViewProjectionBuffer0,
                sc.LightViewProjectionBuffer1,
                sc.LightViewProjectionBuffer2,
                sc.DepthLimitsBuffer,
                sc.LightInfoBuffer,
                sc.CameraInfoBuffer,
                sc.PointLightsBuffer,
                _materialProps.UniformBuffer,
                _textureView,
                gd.Aniso4xSampler,
                _alphaMapView,
                gd.LinearSampler,
                sc.NearShadowMapView,
                sc.MidShadowMapView,
                sc.FarShadowMapView,
                gd.PointSampler));

            _disposeCollector.Add(_mainLayout, _pipeline, _resourceSet);
        }

        private ResourceSet[] CreateShadowMapResourceSets(ResourceFactory factory, CommandList cl, SceneContext sc)
        {
            ResourceSet[] ret = new ResourceSet[3];

            for (int i = 0; i < ret.Length; i++)
            {
                UniformBuffer viewProjBuffer = i == 0 ? sc.LightViewProjectionBuffer0 : i == 1 ? sc.LightViewProjectionBuffer1 : sc.LightViewProjectionBuffer2;
                ret[i] = factory.CreateResourceSet(new ResourceSetDescription(
                    _depthLayout,
                    viewProjBuffer,
                    _worldBuffer));
            }

            return ret;
        }

        public override void DestroyDeviceObjects()
        {
            if (_materialPropsOwned)
            {
                _materialProps.DestroyDeviceObjects();
            }

            _disposeCollector.DisposeAll();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return RenderOrderKey.Create(_pipeline.GetHashCode(), Vector3.Distance(_transform.Position, cameraPosition));
        }

        public override RenderPasses RenderPasses => RenderPasses.AllShadowMap | RenderPasses.Standard;

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            if (_materialPropsOwned)
            {
                _materialProps.FlushChanges(cl);
            }

            if ((renderPass & RenderPasses.AllShadowMap) != 0)
            {
                int shadowMapIndex = renderPass == RenderPasses.ShadowMapNear ? 0 : renderPass == RenderPasses.ShadowMapMid ? 1 : 2;
                RenderShadowMap(cl, sc, shadowMapIndex);
            }
            else if (renderPass == RenderPasses.Standard)
            {
                RenderStandard(cl, sc);
            }
        }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            Matrix4x4 world = _transform.GetTransformMatrix();
            cl.UpdateBuffer(_worldBuffer, 0, ref world);
            cl.UpdateBuffer(_inverseTransposeWorldBuffer, 0, VdUtilities.CalculateInverseTranspose(ref world));
        }

        private void RenderShadowMap(CommandList cl, SceneContext sc, int shadowMapIndex)
        {
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib);
            cl.SetPipeline(_shadowMapPipeline);
            cl.SetResourceSet(_shadowMapResourceSets[shadowMapIndex]);
            cl.Draw((uint)_indexCount, 1, 0, 0, 0);
        }

        private void RenderStandard(CommandList cl, SceneContext sc)
        {
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib);
            cl.SetPipeline(_pipeline);
            cl.SetResourceSet(_resourceSet);
            cl.Draw((uint)_indexCount, 1, 0, 0, 0);
        }
    }
}
