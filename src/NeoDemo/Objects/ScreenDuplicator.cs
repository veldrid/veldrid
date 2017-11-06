using System.Numerics;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo.Objects
{
    internal class ScreenDuplicator : Renderable
    {
        private DisposeCollector _disposeCollector;
        private Pipeline _pipeline;
        private IndexBuffer _ib;
        private VertexBuffer _vb;

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
                    BlendAttachmentDescription.OverrideBlend,
                    BlendAttachmentDescription.OverrideBlend),
                DepthStencilStateDescription.LessEqual,
                RasterizerStateDescription.Default,
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
                        new ShaderStageDescription(ShaderStages.Vertex, ShaderHelper.LoadShader(factory, "ScreenDuplicator", ShaderStages.Vertex), "VS"),
                        new ShaderStageDescription(ShaderStages.Fragment, ShaderHelper.LoadShader(factory, "ScreenDuplicator", ShaderStages.Fragment), "FS"),
                    }),
                new ResourceLayout[] { resourceLayout },
                sc.DuplicatorFramebuffer.OutputDescription);
            _pipeline = factory.CreatePipeline(ref pd);

            _vb = factory.CreateVertexBuffer(new BufferDescription((uint)s_quadVerts.Length * sizeof(float)));
            cl.UpdateBuffer(_vb, 0, s_quadVerts);

            _ib = factory.CreateIndexBuffer(
                new IndexBufferDescription((uint)s_quadIndices.Length * sizeof(ushort), IndexFormat.UInt16));
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
            cl.SetResourceSet(0, sc.MainSceneViewResourceSet);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib);
            cl.Draw(6, 1, 0, 0, 0);
        }

        public override RenderPasses RenderPasses => RenderPasses.Duplicator;

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
