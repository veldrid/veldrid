using ShaderGen;
using System.Numerics;
using System.Runtime.CompilerServices;
using System;
using Vd2.ImageSharp;
using Vd2.Utilities;

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
        private ResourceSet _resourceSet;

        private Pipeline _shadowMapPipeline;
        private ResourceSet[] _shadowMapResourceSets;

        private UniformBuffer _worldBuffer;
        private UniformBuffer _inverseTransposeWorldBuffer;

        private readonly MaterialPropsAndBuffer _materialProps;

        private bool _materialPropsOwned = false;

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

            _worldBuffer = factory.CreateUniformBuffer(new BufferDescription(64));
            _inverseTransposeWorldBuffer = factory.CreateUniformBuffer(new BufferDescription(64));
            if (_materialPropsOwned)
            {
                _materialProps.CreateDeviceObjects(gd, cl, sc);
            }

            _texture = _textureData.CreateDeviceTexture(factory, cl);
            _textureView = factory.CreateTextureView(_texture);
            _alphamapTexture = factory.CreateTexture2D(new TextureDescription(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            RgbaByte color = RgbaByte.White;
            cl.UpdateTexture2D(_alphamapTexture, (IntPtr)(&color), 4, 0, 0, 1, 1, 0, 0);
            _alphaMapView = factory.CreateTextureView(_alphamapTexture);

            VertexLayoutDescription[] shadowDepthVertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
                    new VertexElementDescription("Normal", VertexElementFormat.Float3, VertexElementSemantic.Normal),
                    new VertexElementDescription("TexCoord", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate))
            };

            ShaderStageDescription[] shadowDepthShaderStages = new ShaderStageDescription[]
            {
                new ShaderStageDescription(ShaderStages.Vertex, ShaderHelper.LoadShader(factory, "ShadowDepth", ShaderStages.Vertex), "VS"),
                new ShaderStageDescription(ShaderStages.Fragment, ShaderHelper.LoadShader(factory, "ShadowDepth", ShaderStages.Fragment), "FS"),
            };

            PipelineDescription depthPD = new PipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.LessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(shadowDepthVertexLayouts, shadowDepthShaderStages));
            _shadowMapPipeline = factory.CreatePipeline(ref depthPD);

            _shadowMapResourceSets = CreateShadowMapResourceSets(factory, cl, sc);

            VertexLayoutDescription[] mainVertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
                    new VertexElementDescription("Normal", VertexElementFormat.Float3, VertexElementSemantic.Normal),
                    new VertexElementDescription("TexCoord", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate))
            };

            ShaderStageDescription[] mainShaderStages = new ShaderStageDescription[]
            {
                new ShaderStageDescription(ShaderStages.Vertex, ShaderHelper.LoadShader(factory, "ShadowMain", ShaderStages.Vertex), "VS"),
                new ShaderStageDescription(ShaderStages.Fragment, ShaderHelper.LoadShader(factory, "ShadowMain", ShaderStages.Fragment), "FS"),
            };

            PipelineDescription mainPD = new PipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.LessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(mainVertexLayouts, mainShaderStages));
            _pipeline = factory.CreatePipeline(ref mainPD);

            ResourceLayout mainLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
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
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.Texture2D, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("RegularSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AlphaMap", ResourceKind.Texture2D, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AlphaMapSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapNear", ResourceKind.Texture2D, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapMid", ResourceKind.Texture2D, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapFar", ResourceKind.Texture2D, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(mainLayout,
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
        }

        private ResourceSet[] CreateShadowMapResourceSets(ResourceFactory factory, CommandList cl, SceneContext sc)
        {
            ResourceLayout depthLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                 new ResourceLayoutElementDescription("ViewProjection", ResourceKind.Uniform, ShaderStages.Vertex),
                 new ResourceLayoutElementDescription("World", ResourceKind.Uniform, ShaderStages.Vertex)));
            ResourceSet[] ret = new ResourceSet[3];

            for (int i = 0; i < ret.Length; i++)
            {
                UniformBuffer viewProjBuffer = i == 0 ? sc.LightViewProjectionBuffer0 : i == 1 ? sc.LightViewProjectionBuffer1 : sc.LightViewProjectionBuffer2;
                ret[i] = factory.CreateResourceSet(new ResourceSetDescription(
                    depthLayout,
                    viewProjBuffer,
                    _worldBuffer));
            }

            return ret;
        }

        public override void DestroyDeviceObjects()
        {
            _vb.Dispose();
            _ib.Dispose();
            _worldBuffer.Dispose();
            _inverseTransposeWorldBuffer.Dispose();
            _texture.Dispose();
            _textureView.Dispose();
            if (_materialPropsOwned)
            {
                _materialProps.DestroyDeviceObjects();
            }
            _alphamapTexture.Dispose();
            _alphaMapView.Dispose();
            _pipeline.Dispose();
            _resourceSet.Dispose();
            _shadowMapPipeline.Dispose();
            foreach (ResourceSet set in _shadowMapResourceSets) { set.Dispose(); }
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return RenderOrderKey.Create(_pipeline.GetHashCode(), Vector3.Distance(_transform.Position, cameraPosition));
        }

        public override RenderPasses RenderPasses => RenderPasses.ShadowMap | RenderPasses.Standard;

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            if (_materialPropsOwned)
            {
                _materialProps.FlushChanges(cl);
            }

            if (renderPass == RenderPasses.ShadowMap)
            {
                RenderShadowMap(cl, sc);
            }
            else if (renderPass == RenderPasses.Standard)
            {
                RenderStandard(cl, sc);
            }
        }

        private void RenderShadowMap(CommandList cl, SceneContext sc)
        {
            Matrix4x4 world = _transform.GetTransformMatrix();
            cl.UpdateBuffer(_worldBuffer, 0, ref world);
            cl.UpdateBuffer(_inverseTransposeWorldBuffer, 0, VdUtilities.CalculateInverseTranspose(ref world));

            cl.SetVertexBuffer(0, _vb, VertexPositionNormalTexture.SizeInBytes);
            cl.SetIndexBuffer(_ib);
            cl.SetPipeline(_shadowMapPipeline);
            cl.SetResourceSet(_shadowMapResourceSets[sc.CurrentLightViewProjectionBuffer]);
            cl.Draw((uint)_indexCount, 1, 0, 0, 0);
        }

        private void RenderStandard(CommandList cl, SceneContext sc)
        {
            Matrix4x4 world = _transform.GetTransformMatrix();
            cl.UpdateBuffer(_worldBuffer, 0, ref world);
            cl.UpdateBuffer(_inverseTransposeWorldBuffer, 0, VdUtilities.CalculateInverseTranspose(ref world));

            cl.SetVertexBuffer(0, _vb, VertexPositionNormalTexture.SizeInBytes);
            cl.SetIndexBuffer(_ib);
            cl.SetPipeline(_pipeline);
            cl.SetResourceSet(_resourceSet);
            cl.Draw((uint)_indexCount, 1, 0, 0, 0);
        }
    }
}
