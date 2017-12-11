using System.Numerics;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo.Objects
{
    internal class FullScreenQuad : Renderable
    {
        private DisposeCollector _disposeCollector;
        private Pipeline _pipeline;
        private Buffer _ib;
        private Buffer _vb;
        public bool UseTintedTexture { get; set; }

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            DisposeCollectorResourceFactory factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            _disposeCollector = factory.DisposeCollector;

            ResourceLayout resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                new BlendStateDescription(
                    RgbaFloat.Black,
                    BlendAttachmentDescription.OverrideBlend),
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    new VertexLayoutDescription[]
                    {
                        new VertexLayoutDescription(
                            new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
                    },
                    new ShaderStageDescription[]
                    {
                        new ShaderStageDescription(ShaderStages.Vertex, ShaderHelper.LoadShader(gd, factory, "FullScreenQuad", ShaderStages.Vertex), "VS"),
                        new ShaderStageDescription(ShaderStages.Fragment, ShaderHelper.LoadShader(gd, factory, "FullScreenQuad", ShaderStages.Fragment), "FS"),
                    }),
                new ResourceLayout[] { resourceLayout },
                gd.SwapchainFramebuffer.OutputDescription);
            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _vb = factory.CreateBuffer(new BufferDescription(s_quadVerts.SizeInBytes() * sizeof(float), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, s_quadVerts);

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
            cl.SetGraphicsResourceSet(0, UseTintedTexture ? sc.DuplicatorTargetSet1 : sc.DuplicatorTargetSet0);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.DrawIndexed(6, 1, 0, 0, 0);
        }

        public override RenderPasses RenderPasses => RenderPasses.SwapchainOutput;

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
        }

        private static float[] s_quadVerts = new float[]
        {
            -1, 1, 0, 0,
            1, 1, 1, 0,
            1, -1, 1, 1,
            -1, -1, 0, 1
        };

        private static ushort[] s_quadIndices = new ushort[] { 0, 1, 2, 0, 2, 3 };
    }
}
