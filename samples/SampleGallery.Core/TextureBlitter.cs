using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid.SPIRV;

namespace Veldrid.SampleGallery
{
    public class TextureBlitter : IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly ResourceLayout _rl;
        private readonly ResourceLayout _sampleRegionLayout;
        private readonly Pipeline _pipeline;
        private readonly DeviceBuffer _sampleRegionUB;
        private readonly ResourceSet _sampleRegionSet;
        private Vector4 _lastMinMaxUV;

        public ResourceLayout ResourceLayout => _rl;

        public TextureBlitter(
            GraphicsDevice gd,
            ResourceFactory factory,
            OutputDescription outputDesc,
            bool srgbOutput)
        {
            _gd = gd;

            SpecializationConstant[] specConstants = new[]
            {
                new SpecializationConstant(0, srgbOutput),
                new SpecializationConstant(1, gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES)
            };

            string setName = srgbOutput ? "TextureBlitter_SRGB" : "TextureBlitter";
            Shader[] shaders = ShaderUtil.LoadEmbeddedShaderSet(typeof(TextureBlitter).Assembly, factory, setName);

            _rl = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Input", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("InputSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            _sampleRegionLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SampleRegionInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleStrip,
                new ShaderSetDescription(
                    Array.Empty<VertexLayoutDescription>(),
                    new[] { shaders[0], shaders[1] },
                    specConstants),
                new[] { _rl, _sampleRegionLayout },
                outputDesc));

            _sampleRegionUB = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));
            _sampleRegionSet = factory.CreateResourceSet(new ResourceSetDescription(_sampleRegionLayout, _sampleRegionUB));

            _lastMinMaxUV = new Vector4(0, 0, 1, 1);
            gd.UpdateBuffer(_sampleRegionUB, 0, _lastMinMaxUV);
        }

        public void Render(CommandBuffer cb, ResourceSet rs, Vector2 minUV, Vector2 maxUV)
        {
            Vector4 newVal = new Vector4(minUV.X, minUV.Y, maxUV.X, maxUV.Y);
            if (_lastMinMaxUV != newVal)
            {
                _lastMinMaxUV = newVal;
                cb.UpdateBuffer(_sampleRegionUB, 0, newVal);
            }

            cb.BindPipeline(_pipeline);
            cb.BindGraphicsResourceSet(0, rs);
            cb.BindGraphicsResourceSet(1, _sampleRegionSet);
            cb.Draw(4);
        }

        public void Dispose()
        {
            _rl.Dispose();
            _pipeline.Dispose();
            _sampleRegionUB.Dispose();
            _sampleRegionSet.Dispose();
            _sampleRegionLayout.Dispose();
        }
    }
}
