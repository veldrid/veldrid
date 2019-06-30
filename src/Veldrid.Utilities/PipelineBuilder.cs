using System.Linq;

namespace Veldrid.Utilities
{
    public struct PipelineBuilder
    {
        private BlendStateDescription _blendState;
        private DepthStencilStateDescription _depthStencilState;
        private RasterizerStateDescription _rasterizerState;
        private PrimitiveTopology _primitiveTopology;
        private ShaderSetDescription _shaderSet;
        private ResourceLayout[] _resourceLayouts;
        private OutputDescription _outputs;

        public PipelineBuilder Clone()
        {
            PipelineBuilder clone = this;

            Clone(ref clone._shaderSet.Shaders);
            Clone(ref clone._shaderSet.Specializations);
            Clone(ref clone._shaderSet.VertexLayouts);

            Clone(ref clone._resourceLayouts);

            Clone(ref _outputs.ColorAttachments);

            return clone;
        }

        public Pipeline Build(ResourceFactory factory)
        {
            GraphicsPipelineDescription desc = new GraphicsPipelineDescription(
                _blendState,
                _depthStencilState,
                _rasterizerState,
                _primitiveTopology,
                _shaderSet,
                _resourceLayouts,
                _outputs,
                ResourceBindingModel.Improved);
            return factory.CreateGraphicsPipeline(desc);
        }

        private static void Clone<T>(ref T[] array)
        {
            array = (T[])array.Clone();
        }

        private void Clone(ref VertexLayoutDescription[] original)
        {
            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[original.Length];
            for (int i = 0; i < original.Length; i++)
            {
                vertexLayouts[i].Stride = original[i].Stride;
                vertexLayouts[i].InstanceStepRate = original[i].InstanceStepRate;
                vertexLayouts[i].Elements = (VertexElementDescription[])original[i].Elements.Clone();
            }

            original = vertexLayouts;
        }

        public PipelineBuilder Shaders(Shader vertex, Shader fragment)
        {
            PipelineBuilder c = Clone();
            c._shaderSet.Shaders = new[] { vertex, fragment };
            return c;
        }

        public PipelineBuilder Outputs(Framebuffer fb)
        {
            PipelineBuilder c = Clone();
            c._outputs = new OutputDescription();
            if (fb.DepthTarget != null)
            {
                c._outputs.DepthAttachment = new OutputAttachmentDescription(fb.DepthTarget.Value.Target.Format);
                c._outputs.SampleCount = fb.DepthTarget.Value.Target.SampleCount;
            }
            if (fb.ColorTargets.Count > 0)
            {
                c._outputs.ColorAttachments = fb.ColorTargets
                    .Select(fa => new OutputAttachmentDescription(fa.Target.Format)).ToArray();
                c._outputs.SampleCount = fb.ColorTargets[0].Target.SampleCount;
            }

            return c;
        }

        public PipelineBuilder Topology(PrimitiveTopology topology)
        {
            PipelineBuilder c = Clone();
            c._primitiveTopology = topology;
            return c;
        }
    }
}
