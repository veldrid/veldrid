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
        private readonly string _name;
        private readonly MeshData _meshData;
        private readonly ImageSharpTexture _textureData;
        private readonly ImageSharpTexture _alphaTextureData;
        private readonly Transform _transform = new Transform();

        private BoundingBox _centeredBounds;
        private DeviceBuffer _vb;
        private DeviceBuffer _ib;
        private int _indexCount;
        private Texture _texture;
        private TextureView _textureView;
        private Texture _alphamapTexture;
        private TextureView _alphaMapView;

        private Pipeline _pipeline;
        private Pipeline _pipelineFrontCull;
        private ResourceSet _mainProjViewRS;
        private ResourceSet _mainSharedRS;
        private ResourceSet _mainPerObjectRS;
        private ResourceSet _reflectionRS;
        private ResourceSet _noReflectionRS;
        private Pipeline _shadowMapPipeline;
        private ResourceSet[] _shadowMapResourceSets;

        private DeviceBuffer _worldBuffer;
        private DeviceBuffer _inverseTransposeWorldBuffer;

        private readonly DisposeCollector _disposeCollector = new DisposeCollector();

        private readonly MaterialPropsAndBuffer _materialProps;

        private bool _materialPropsOwned = false;

        public MaterialProperties MaterialProperties { get => _materialProps.Properties; set { _materialProps.Properties = value; } }

        public Transform Transform => _transform;

        public TexturedMesh(string name, MeshData meshData, ImageSharpTexture textureData, ImageSharpTexture alphaTexture, MaterialPropsAndBuffer materialProps)
        {
            _name = name;
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
            _vb.Name = _name + "_VB";
            _ib = _meshData.CreateIndexBuffer(disposeFactory, cl, out _indexCount);
            _ib.Name = _name + "_IB";

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
                _texture = disposeFactory.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
                RgbaByte color = RgbaByte.Pink;
                gd.UpdateTexture(_texture, (IntPtr)(&color), 4, 0, 0, 0, 1, 1, 1, 0, 0);
            }

            _textureView = StaticResourceCache.GetTextureView(gd.ResourceFactory, _texture);

            if (_alphaTextureData != null)
            {
                _alphamapTexture = _alphaTextureData.CreateDeviceTexture(gd, disposeFactory);
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

            Shader depthVS = StaticResourceCache.GetShader(gd, gd.ResourceFactory, "ShadowDepth", ShaderStages.Vertex, "VS");
            Shader depthFS = StaticResourceCache.GetShader(gd, gd.ResourceFactory, "ShadowDepth", ShaderStages.Fragment, "FS");

            ResourceLayout projViewCombinedLayout = StaticResourceCache.GetResourceLayout(
                gd.ResourceFactory,
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ViewProjection", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceLayout worldLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory, new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            GraphicsPipelineDescription depthPD = new GraphicsPipelineDescription(
                BlendStateDescription.Empty,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(shadowDepthVertexLayouts, new[] { depthVS, depthFS }),
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

            Shader mainVS = StaticResourceCache.GetShader(gd, gd.ResourceFactory, "ShadowMain", ShaderStages.Vertex, "VS");
            Shader mainFS = StaticResourceCache.GetShader(gd, gd.ResourceFactory, "ShadowMain", ShaderStages.Fragment, "FS");

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

            ResourceLayout reflectionLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory, new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ReflectionMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ReflectionSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ReflectionViewProj", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ClipPlaneInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

            GraphicsPipelineDescription mainPD = new GraphicsPipelineDescription(
                _alphamapTexture != null ? BlendStateDescription.SingleAlphaBlend : BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(mainVertexLayouts, new[] { mainVS, mainFS }),
                new ResourceLayout[] { projViewLayout, mainSharedLayout, mainPerObjectLayout, reflectionLayout },
                sc.MainSceneFramebuffer.OutputDescription);
            _pipeline = StaticResourceCache.GetPipeline(gd.ResourceFactory, ref mainPD);
            mainPD.RasterizerState.CullMode = FaceCullMode.Front;
            mainPD.Outputs = sc.ReflectionFramebuffer.OutputDescription;
            _pipelineFrontCull = StaticResourceCache.GetPipeline(gd.ResourceFactory, ref mainPD);

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

            _reflectionRS = StaticResourceCache.GetResourceSet(gd.ResourceFactory, new ResourceSetDescription(reflectionLayout,
                _alphaMapView, // Doesn't really matter -- just don't bind the actual reflection map since it's being rendered to.
                gd.PointSampler,
                sc.ReflectionViewProjBuffer,
                sc.MirrorClipPlaneBuffer));

            _noReflectionRS = StaticResourceCache.GetResourceSet(gd.ResourceFactory, new ResourceSetDescription(reflectionLayout,
                sc.ReflectionColorView,
                gd.PointSampler,
                sc.ReflectionViewProjBuffer,
                sc.NoClipPlaneBuffer));
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
                DeviceBuffer viewProjBuffer = i == 0 ? sc.LightViewProjectionBuffer0 : i == 1 ? sc.LightViewProjectionBuffer1 : sc.LightViewProjectionBuffer2;
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
                    return RenderPasses.AllShadowMap | RenderPasses.AlphaBlend | RenderPasses.ReflectionMap;
                }
                else
                {
                    return RenderPasses.AllShadowMap | RenderPasses.Standard | RenderPasses.ReflectionMap;
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
                RenderStandard(cl, sc, false);
            }
            else if (renderPass == RenderPasses.ReflectionMap)
            {
                RenderStandard(cl, sc, true);
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

        private void RenderStandard(CommandList cl, SceneContext sc, bool reflectionPass)
        {
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(reflectionPass ? _pipelineFrontCull : _pipeline);
            cl.SetGraphicsResourceSet(0, _mainProjViewRS);
            cl.SetGraphicsResourceSet(1, _mainSharedRS);
            cl.SetGraphicsResourceSet(2, _mainPerObjectRS);
            cl.SetGraphicsResourceSet(3, reflectionPass ? _reflectionRS : _noReflectionRS);
            cl.DrawIndexed((uint)_indexCount, 1, 0, 0, 0);
        }
    }
}
