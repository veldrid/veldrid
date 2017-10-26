using System;

namespace Vd2
{
    public struct PipelineDescription : IEquatable<PipelineDescription>
    {
        public BlendStateDescription BlendState;
        public DepthStencilStateDescription DepthStencilState;
        public RasterizerStateDescription RasterizerState;
        public PrimitiveTopology PrimitiveTopology;
        public ShaderSetDescription ShaderSet;
        public ResourceLayout ResourceLayout; // TODO: Allow multiple resource layouts
        public OutputDescription Outputs;

        public PipelineDescription(
            BlendStateDescription blendState,
            DepthStencilStateDescription depthStencilStateDescription,
            RasterizerStateDescription rasterizerState,
            PrimitiveTopology primitiveTopology,
            ShaderSetDescription shaderSet,
            ResourceLayout resourceLayout,
            OutputDescription outputs)
        {
            BlendState = blendState;
            DepthStencilState = depthStencilStateDescription;
            RasterizerState = rasterizerState;
            PrimitiveTopology = primitiveTopology;
            ShaderSet = shaderSet;
            ResourceLayout = resourceLayout;
            Outputs = outputs;
        }

        public bool Equals(PipelineDescription other)
        {
            return BlendState.Equals(other.BlendState)
                && DepthStencilState.Equals(other.DepthStencilState)
                && RasterizerState.Equals(other.RasterizerState)
                && PrimitiveTopology == other.PrimitiveTopology
                && ShaderSet.Equals(other.ShaderSet)
                && ResourceLayout.Equals(other.ResourceLayout)
                && Outputs.Equals(other.Outputs);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(
                BlendState.GetHashCode(),
                DepthStencilState.GetHashCode(),
                RasterizerState.GetHashCode(),
                PrimitiveTopology.GetHashCode(),
                ShaderSet.GetHashCode(),
                ResourceLayout.GetHashCode(),
                Outputs.GetHashCode());
        }
    }
}
