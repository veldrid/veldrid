using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using Veldrid.Utilities;
using Veldrid.ImageSharp;

namespace Veldrid.NeoDemo.Objects
{
    public class Skybox : Renderable
    {
        private readonly Image<Rgba32> _front;
        private readonly Image<Rgba32> _back;
        private readonly Image<Rgba32> _left;
        private readonly Image<Rgba32> _right;
        private readonly Image<Rgba32> _top;
        private readonly Image<Rgba32> _bottom;

        // Context objects
        private DeviceBuffer _vb;
        private DeviceBuffer _ib;
        private Pipeline _pipeline;
        private Pipeline _reflectionPipeline;
        private ResourceSet _resourceSet;
        private readonly DisposeCollector _disposeCollector = new DisposeCollector();

        public Skybox(
            Image<Rgba32> front, Image<Rgba32> back, Image<Rgba32> left,
            Image<Rgba32> right, Image<Rgba32> top, Image<Rgba32> bottom)
        {
            _front = front;
            _back = back;
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
        }

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;

            _vb = factory.CreateBuffer(new BufferDescription(s_vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, s_vertices);

            _ib = factory.CreateBuffer(new BufferDescription(s_indices.SizeInBytes(), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, s_indices);

            ImageSharpCubemapTexture imageSharpCubemapTexture = new ImageSharpCubemapTexture(_right, _left, _top, _bottom, _back, _front, false);

            Texture textureCube = imageSharpCubemapTexture.CreateDeviceTexture(gd, factory);
            TextureView textureView = factory.CreateTextureView(new TextureViewDescription(textureCube));

            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
            };

            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "Skybox");

            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("CubeTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("CubeSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                gd.IsDepthRangeZeroToOne ? DepthStencilStateDescription.DepthOnlyGreaterEqual : DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(vertexLayouts, new[] { vs, fs }, ShaderHelper.GetSpecializations(gd)),
                new ResourceLayout[] { _layout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);
            pd.Outputs = sc.ReflectionFramebuffer.OutputDescription;
            _reflectionPipeline = factory.CreateGraphicsPipeline(ref pd);

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                _layout,
                sc.ProjectionMatrixBuffer,
                sc.ViewMatrixBuffer,
                textureView,
                gd.PointSampler));

            _disposeCollector.Add(_vb, _ib, textureCube, textureView, _layout, _pipeline, _reflectionPipeline, _resourceSet, vs, fs);
        }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
        }

        public static Skybox LoadDefaultSkybox()
        {
            return new Skybox(
                Image.Load<Rgba32>(AssetHelper.GetPath("Textures/cloudtop/cloudtop_ft.png")),
                Image.Load<Rgba32>(AssetHelper.GetPath("Textures/cloudtop/cloudtop_bk.png")),
                Image.Load<Rgba32>(AssetHelper.GetPath("Textures/cloudtop/cloudtop_lf.png")),
                Image.Load<Rgba32>(AssetHelper.GetPath("Textures/cloudtop/cloudtop_rt.png")),
                Image.Load<Rgba32>(AssetHelper.GetPath("Textures/cloudtop/cloudtop_up.png")),
                Image.Load<Rgba32>(AssetHelper.GetPath("Textures/cloudtop/cloudtop_dn.png")));
        }

        public override void DestroyDeviceObjects()
        {
            _disposeCollector.DisposeAll();
        }

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(renderPass == RenderPasses.ReflectionMap ? _reflectionPipeline : _pipeline);
            cl.SetGraphicsResourceSet(0, _resourceSet);
            Texture texture = renderPass == RenderPasses.ReflectionMap ? sc.ReflectionColorTexture : sc.MainSceneColorTexture;
            float depth = gd.IsDepthRangeZeroToOne ? 0 : 1;
            cl.SetViewport(0, new Viewport(0, 0, texture.Width, texture.Height, depth, depth));
            cl.DrawIndexed((uint)s_indices.Length, 1, 0, 0, 0);
            cl.SetViewport(0, new Viewport(0, 0, texture.Width, texture.Height, 0, 1));
        }

        public override RenderPasses RenderPasses => RenderPasses.Standard | RenderPasses.ReflectionMap;

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return new RenderOrderKey(ulong.MaxValue);
        }

        private static readonly VertexPosition[] s_vertices = new VertexPosition[]
        {
            // Top
            new VertexPosition(new Vector3(-20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,20.0f,20.0f)),
            // Bottom
            new VertexPosition(new Vector3(-20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,-20.0f)),
            // Left
            new VertexPosition(new Vector3(-20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,-20.0f)),
            // Right
            new VertexPosition(new Vector3(20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,20.0f)),
            // Back
            new VertexPosition(new Vector3(20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,-20.0f)),
            // Front
            new VertexPosition(new Vector3(-20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,20.0f)),
        };

        private static readonly ushort[] s_indices = new ushort[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23,
        };
        private ResourceLayout _layout;
    }
}
