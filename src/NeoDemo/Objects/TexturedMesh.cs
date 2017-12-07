using ShaderGen;
using System.Numerics;
using System.Runtime.CompilerServices;
using System;
using Veldrid.ImageSharp;
using Veldrid.Utilities;
using System.Collections.Generic;

namespace Veldrid.NeoDemo.Objects
{
    public class TexturedMesh : CullRenderable
    {
        private readonly MeshData _meshData;
        private readonly ImageSharpTexture _textureData;
        private readonly ImageSharpTexture _alphaTextureData;
        private readonly Transform _transform = new Transform();

        private BoundingBox _centeredBounds;
        private Buffer _vb;
        private Buffer _ib;
        private int _indexCount;
        private Texture _texture;
        private TextureView _textureView;
        private Texture _alphamapTexture;
        private TextureView _alphaMapView;

        private Pipeline _pipeline;
        private ResourceSet _mainProjViewRS;
        private ResourceSet _mainSharedRS;
        private ResourceSet _mainPerObjectRS;
        private Pipeline _shadowMapPipeline;
        private ResourceSet[] _shadowMapResourceSets;

        private Buffer _worldBuffer;
        private Buffer _inverseTransposeWorldBuffer;

        private readonly DisposeCollector _disposeCollector = new DisposeCollector();

        private readonly MaterialPropsAndBuffer _materialProps;

        private bool _materialPropsOwned = false;

        public MaterialProperties MaterialProperties { get => _materialProps.Properties; set { _materialProps.Properties = value; } }

        public Transform Transform => _transform;

        public TexturedMesh(MeshData meshData, ImageSharpTexture textureData, ImageSharpTexture alphaTexture, MaterialPropsAndBuffer materialProps)
        {
            _meshData = meshData;
            _centeredBounds = meshData.GetBoundingBox();
            _textureData = textureData;
            _alphaTextureData = alphaTexture;
            _materialProps = materialProps;
        }

        public override BoundingBox BoundingBox => BoundingBox.Transform(_centeredBounds, _transform.GetTransformMatrix());

        public unsafe override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory disposeFactory = new DisposeCollectorResourceFactory(gd.ResourceFactory, _disposeCollector);
            _vb = _meshData.CreateVertexBuffer(disposeFactory, cl);
            _ib = _meshData.CreateIndexBuffer(disposeFactory, cl, out _indexCount);

            _worldBuffer = disposeFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _inverseTransposeWorldBuffer = disposeFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            if (_materialPropsOwned)
            {
                _materialProps.CreateDeviceObjects(gd, cl, sc);
            }

            if (_textureData != null)
            {
                _texture = StaticResourceCache.GetTexture2D(gd, gd.ResourceFactory, _textureData);
            }
            else
            {
                _texture = disposeFactory.CreateTexture(new TextureDescription(1, 1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
                RgbaByte color = RgbaByte.Pink;
                gd.UpdateTexture(_texture, (IntPtr)(&color), 4, 0, 0, 0, 1, 1, 1, 0, 0);
            }

            _textureView = StaticResourceCache.GetTextureView(gd.ResourceFactory, _texture);

            if (_alphaTextureData != null)
            {
                _alphamapTexture = _alphaTextureData.CreateDeviceTexture(gd , disposeFactory);
            }
            else
            {
                _alphamapTexture = StaticResourceCache.GetPinkTexture(gd, gd.ResourceFactory);
            }
            _alphaMapView = StaticResourceCache.GetTextureView(gd.ResourceFactory, _alphamapTexture);

            VertexLayoutDescription[] shadowDepthVertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            };

            Shader depthVS = StaticResourceCache.GetShader(gd, gd.ResourceFactory, "ShadowDepth", ShaderStages.Vertex);
            Shader depthFS = StaticResourceCache.GetShader(gd, gd.ResourceFactory, "ShadowDepth", ShaderStages.Fragment);
            ShaderStageDescription[] shadowDepthShaderStages = new ShaderStageDescription[]
            {
                new ShaderStageDescription(depthVS, "VS"),
                new ShaderStageDescription(depthFS, "FS"),
            };

            ResourceLayout projViewCombinedLayout = StaticResourceCache.GetResourceLayout(
                gd.ResourceFactory,
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ViewProjection", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceLayout worldLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory, new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            GraphicsPipelineDescription depthPD = new GraphicsPipelineDescription(
                BlendStateDescription.Empty,
                DepthStencilStateDescription.LessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(shadowDepthVertexLayouts, shadowDepthShaderStages),
                new ResourceLayout[] { projViewCombinedLayout, worldLayout },
                DemoOutputsDescriptions.ShadowMapPass);
            _shadowMapPipeline = StaticResourceCache.GetPipeline(gd.ResourceFactory, ref depthPD);

            _shadowMapResourceSets = CreateShadowMapResourceSets(gd.ResourceFactory, disposeFactory, cl, sc, projViewCombinedLayout, worldLayout);

            VertexLayoutDescription[] mainVertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            };

            Shader mainVS = StaticResourceCache.GetShader(gd, gd.ResourceFactory, "ShadowMain", ShaderStages.Vertex);
            Shader mainFS = StaticResourceCache.GetShader(gd, gd.ResourceFactory, "ShadowMain", ShaderStages.Fragment);
            ShaderStageDescription[] mainShaderStages = new ShaderStageDescription[]
            {
                new ShaderStageDescription(mainVS, "VS"),
                new ShaderStageDescription(mainFS, "FS"),
            };

            ResourceLayout projViewLayout = StaticResourceCache.GetResourceLayout(
                gd.ResourceFactory,
                StaticResourceCache.ProjViewLayoutDescription);

            ResourceLayout mainSharedLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory, new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("LightViewProjection1", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightViewProjection2", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightViewProjection3", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("DepthLimits", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("PointLights", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));

            ResourceLayout mainPerObjectLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory, new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("InverseTransposeWorld", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("MaterialProperties", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("RegularSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AlphaMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AlphaMapSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapNear", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapMid", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapFar", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            GraphicsPipelineDescription mainPD = new GraphicsPipelineDescription(
                _alphamapTexture != null ? BlendStateDescription.SingleAlphaBlend : BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.LessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(mainVertexLayouts, mainShaderStages),
                new ResourceLayout[] { projViewLayout, mainSharedLayout, mainPerObjectLayout },
                sc.MainSceneFramebuffer.OutputDescription);
            _pipeline = StaticResourceCache.GetPipeline(gd.ResourceFactory, ref mainPD);

            _mainProjViewRS = StaticResourceCache.GetResourceSet(gd.ResourceFactory, new ResourceSetDescription(projViewLayout,
                sc.ProjectionMatrixBuffer,
                sc.ViewMatrixBuffer));

            _mainSharedRS = StaticResourceCache.GetResourceSet(gd.ResourceFactory, new ResourceSetDescription(mainSharedLayout,
                sc.LightViewProjectionBuffer0,
                sc.LightViewProjectionBuffer1,
                sc.LightViewProjectionBuffer2,
                sc.DepthLimitsBuffer,
                sc.LightInfoBuffer,
                sc.CameraInfoBuffer,
                sc.PointLightsBuffer));

            _mainPerObjectRS = disposeFactory.CreateResourceSet(new ResourceSetDescription(mainPerObjectLayout,
                _worldBuffer,
                _inverseTransposeWorldBuffer,
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

        private ResourceSet[] CreateShadowMapResourceSets(
            ResourceFactory sharedFactory,
            ResourceFactory disposeFactory,
            CommandList cl,
            SceneContext sc,
            ResourceLayout projViewLayout,
            ResourceLayout worldLayout)
        {
            ResourceSet[] ret = new ResourceSet[6];

            for (int i = 0; i < 3; i++)
            {
                Buffer viewProjBuffer = i == 0 ? sc.LightViewProjectionBuffer0 : i == 1 ? sc.LightViewProjectionBuffer1 : sc.LightViewProjectionBuffer2;
                ret[i * 2] = StaticResourceCache.GetResourceSet(sharedFactory, new ResourceSetDescription(
                    projViewLayout,
                    viewProjBuffer));
                ResourceSet worldRS = disposeFactory.CreateResourceSet(new ResourceSetDescription(
                    worldLayout,
                    _worldBuffer));
                ret[i * 2 + 1] = worldRS;
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

        public override RenderPasses RenderPasses
        {
            get
            {
                if (_alphaTextureData != null)
                {
                    return RenderPasses.AllShadowMap | RenderPasses.AlphaBlend;
                }
                else
                {
                    return RenderPasses.AllShadowMap | RenderPasses.Standard;
                }
            }
        }

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
            else if (renderPass == RenderPasses.Standard || renderPass == RenderPasses.AlphaBlend)
            {
                RenderStandard(cl, sc);
            }
        }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            Matrix4x4 world = _transform.GetTransformMatrix();
            gd.UpdateBuffer(_worldBuffer, 0, ref world);
            gd.UpdateBuffer(_inverseTransposeWorldBuffer, 0, VdUtilities.CalculateInverseTranspose(ref world));
        }

        private void RenderShadowMap(CommandList cl, SceneContext sc, int shadowMapIndex)
        {
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(_shadowMapPipeline);
            cl.SetGraphicsResourceSet(0, _shadowMapResourceSets[shadowMapIndex * 2]);
            cl.SetGraphicsResourceSet(1, _shadowMapResourceSets[shadowMapIndex * 2 + 1]);
            cl.DrawIndexed((uint)_indexCount, 1, 0, 0, 0);
        }

        private void RenderStandard(CommandList cl, SceneContext sc)
        {
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _mainProjViewRS);
            cl.SetGraphicsResourceSet(1, _mainSharedRS);
            cl.SetGraphicsResourceSet(2, _mainPerObjectRS);
            cl.DrawIndexed((uint)_indexCount, 1, 0, 0, 0);
        }
    }
}
