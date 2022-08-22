using System.Numerics;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo.Objects
{
    internal class ScreenDuplicator : Renderable
    {
        private DisposeCollector _disposeCollector;
        private Pipeline _pipeline;
        private DeviceBuffer _ib;
        private DeviceBuffer _vb;

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            DisposeCollectorResourceFactory factory = new(gd.ResourceFactory);
            _disposeCollector = factory.DisposeCollector;

            ResourceLayout resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "ScreenDuplicator");

            BlendAttachmentDescription blend = BlendAttachmentDescription.OverrideBlend;
            blend.ColorWriteMask = sc.MainSceneMask;

            GraphicsPipelineDescription pd = new(
                new BlendStateDescription(
                    RgbaFloat.Black,
                    blend, 
                    blend),
                gd.IsDepthRangeZeroToOne ? DepthStencilStateDescription.DepthOnlyGreaterEqual : DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    new[]
                    {
                        new VertexLayoutDescription(
                            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
                    },
                    new[] { vs, fs, },
                    ShaderHelper.GetSpecializations(gd)),
                new ResourceLayout[] { resourceLayout },
                sc.DuplicatorFramebuffer.OutputDescription);
            _pipeline = factory.CreateGraphicsPipeline(pd);

            float[] verts = Util.GetFullScreenQuadVerts(gd);

            _vb = factory.CreateBuffer(new BufferDescription(verts.SizeInBytes(), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, verts);

            _ib = factory.CreateBuffer(
                new BufferDescription(s_quadIndices.SizeInBytes(), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, s_quadIndices);
        }

        public override void DestroyDeviceObjects()
        {
            _disposeCollector.DisposeAll();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return new RenderOrderKey();
        }

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, sc.MainSceneViewResourceSet);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.DrawIndexed(6, 1, 0, 0, 0);
        }

        public override RenderPasses RenderPasses => RenderPasses.Duplicator;

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
        }

        private static ushort[] s_quadIndices = new ushort[] { 0, 1, 2, 0, 2, 3 };
    }
}
