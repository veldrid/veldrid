using System.Numerics;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo.Objects
{
    internal class FullScreenQuad : Renderable
    {
        private DisposeCollector _disposeCollector;
        private Pipeline _pipeline;
        private IndexBuffer _ib;
        private VertexBuffer _vb;
        public bool UseTintedTexture { get; set; }

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            DisposeCollectorResourceFactory factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            _disposeCollector = factory.DisposeCollector;

            ResourceLayout resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.Texture, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            PipelineDescription pd = new PipelineDescription(
                new BlendStateDescription(
                    RgbaFloat.Black,
                    BlendAttachmentDescription.OverrideBlend),
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.None, TriangleFillMode.Solid, FrontFace.Clockwise, true, true),
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
                        new ShaderStageDescription(ShaderStages.Vertex, ShaderHelper.LoadShader(factory, "FullScreenQuad", ShaderStages.Vertex), "VS"),
                        new ShaderStageDescription(ShaderStages.Fragment, ShaderHelper.LoadShader(factory, "FullScreenQuad", ShaderStages.Fragment), "FS"),
                    }),
                new ResourceLayout[] { resourceLayout },
                gd.SwapchainFramebuffer.OutputDescription);
            _pipeline = factory.CreatePipeline(ref pd);

            _vb = factory.CreateVertexBuffer(new BufferDescription(s_quadVerts.SizeInBytes() * sizeof(float)));
            cl.UpdateBuffer(_vb, 0, s_quadVerts);

            _ib = factory.CreateIndexBuffer(
                new IndexBufferDescription(s_quadIndices.SizeInBytes(), IndexFormat.UInt16));
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
            cl.SetResourceSet(0, UseTintedTexture ? sc.DuplicatorTargetSet1 : sc.DuplicatorTargetSet0);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib);
            cl.Draw(6, 1, 0, 0, 0);
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
