using System;
using System.Numerics;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo.Objects
{
    public class InfiniteGrid : CullRenderable
    {
        private readonly BoundingBox _boundingBox = new BoundingBox(new Vector3(-1000, -1, -1000), new Vector3(1000, 1, 1000));

        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Pipeline _pipeline;
        private ResourceSet _resourceSet;

        private readonly DisposeCollector _disposeCollector = new DisposeCollector();

        public unsafe override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;
            _vb = factory.CreateVertexBuffer(new BufferDescription(VertexPosition.SizeInBytes * 4));
            cl.UpdateBuffer(_vb, 0, new VertexPosition[]
            {
                new VertexPosition(new Vector3(-1000, 0, -1000)),
                new VertexPosition(new Vector3(+1000, 0, -1000)),
                new VertexPosition(new Vector3(+1000, 0, +1000)),
                new VertexPosition(new Vector3(-1000, 0, +1000)),
            });

            _ib = factory.CreateIndexBuffer(new IndexBufferDescription(6 * 2, IndexFormat.UInt16));
            cl.UpdateBuffer(_ib, 0, new ushort[] { 0, 1, 2, 0, 2, 3 });

            const int gridSize = 64;
            RgbaByte borderColor = new RgbaByte(255, 255, 255, 150);
            RgbaByte[] pixels = CreateGridTexturePixels(gridSize, 1, borderColor, new RgbaByte());
            Texture2D gridTexture = factory.CreateTexture2D(new TextureDescription(gridSize, gridSize, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            fixed (RgbaByte* pixelsPtr = pixels)
            {
                cl.UpdateTexture2D(gridTexture, (IntPtr)pixelsPtr, pixels.SizeInBytes(), 0, 0, gridSize, gridSize, 0, 0);
            }

            TextureView textureView = factory.CreateTextureView(new TextureViewDescription(gridTexture));

            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3))
            };

            Shader gridVS = ShaderHelper.LoadShader(factory, "Grid", ShaderStages.Vertex);
            Shader gridFS = ShaderHelper.LoadShader(factory, "Grid", ShaderStages.Fragment);
            ShaderStageDescription[] shaderStages = new ShaderStageDescription[]
            {
                new ShaderStageDescription(ShaderStages.Vertex, gridVS, "VS"),
                new ShaderStageDescription(ShaderStages.Fragment, gridFS, "FS"),
            };

            ResourceLayout layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("View", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("GridTexture", ResourceKind.Texture, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("GridSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            PipelineDescription pd = new PipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.LessEqual,
                new RasterizerStateDescription(FaceCullMode.None, TriangleFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(vertexLayouts, shaderStages),
                new ResourceLayout[] { layout },
                gd.SwapchainFramebuffer.OutputDescription);

            _pipeline = factory.CreatePipeline(ref pd);

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                layout,
                sc.ProjectionMatrixBuffer,
                sc.ViewMatrixBuffer,
                textureView,
                gd.PointSampler));

            _disposeCollector.Add(_vb, _ib, gridTexture, textureView, gridVS, gridFS, layout, _pipeline, _resourceSet);
        }

        public override void DestroyDeviceObjects()
        {
            _disposeCollector.DisposeAll();
        }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
        }

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            cl.SetPipeline(_pipeline);
            cl.SetResourceSet(0, _resourceSet);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib);
            cl.Draw(6, 1, 0, 0, 0);
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return RenderOrderKey.Create(_pipeline.GetHashCode(), cameraPosition.Length());
        }

        public override RenderPasses RenderPasses => RenderPasses.AlphaBlend;

        public override BoundingBox BoundingBox => _boundingBox;

        private RgbaByte[] CreateGridTexturePixels(int dimensions, int borderPixels, RgbaByte borderColor, RgbaByte backgroundColor)
        {
            RgbaByte[] ret = new RgbaByte[dimensions * dimensions];

            for (int y = 0; y < dimensions; y++)
            {
                for (int x = 0; x < dimensions; x++)
                {
                    if ((y < borderPixels) || (dimensions - 1 - y < borderPixels)
                        || (x < borderPixels) || (dimensions - 1 - x < borderPixels))
                    {
                        ret[x + (y * dimensions)] = borderColor;
                    }
                    else
                    {
                        ret[x + (y * dimensions)] = backgroundColor;
                    }
                }
            }

            return ret;
        }
    }
}
