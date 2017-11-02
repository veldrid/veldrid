using System;

namespace Veldrid
{
    public struct PipelineDescription : IEquatable<PipelineDescription>
    {
        public BlendStateDescription BlendState;
        public DepthStencilStateDescription DepthStencilState;
        public RasterizerStateDescription RasterizerState;
        public PrimitiveTopology PrimitiveTopology;
        public ShaderSetDescription ShaderSet;
        public ResourceLayout[] ResourceLayouts;
        public OutputDescription Outputs;

        public PipelineDescription(
            BlendStateDescription blendState,
            DepthStencilStateDescription depthStencilStateDescription,
            RasterizerStateDescription rasterizerState,
            PrimitiveTopology primitiveTopology,
            ShaderSetDescription shaderSet,
            ResourceLayout[] resourceLayouts,
            OutputDescription outputs)
        {
            BlendState = blendState;
            DepthStencilState = depthStencilStateDescription;
            RasterizerState = rasterizerState;
            PrimitiveTopology = primitiveTopology;
            ShaderSet = shaderSet;
            ResourceLayouts = resourceLayouts;
            Outputs = outputs;
        }

        public bool Equals(PipelineDescription other)
        {
            return BlendState.Equals(other.BlendState)
                && DepthStencilState.Equals(other.DepthStencilState)
                && RasterizerState.Equals(other.RasterizerState)
                && PrimitiveTopology == other.PrimitiveTopology
                && ShaderSet.Equals(other.ShaderSet)
                && Util.ArrayEquals(ResourceLayouts, other.ResourceLayouts)
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
                HashHelper.Array(ResourceLayouts),
                Outputs.GetHashCode());
        }
    }
}
