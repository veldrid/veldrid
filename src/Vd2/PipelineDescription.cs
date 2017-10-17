namespace Vd2
{
    public struct PipelineDescription
    {
        public BlendStateDescription BlendState;
        public DepthStencilStateDescription DepthStencilStateDescription;
        public RasterizerStateDescription RasterizerState;
        public PrimitiveTopology PrimitiveTopology;
        public ShaderSetDescription ShaderSet;

        public PipelineDescription(
            BlendStateDescription blendState,
            DepthStencilStateDescription depthStencilStateDescription,
            RasterizerStateDescription rasterizerState,
            PrimitiveTopology primitiveTopology,
            ShaderSetDescription shaderSet)
        {
            BlendState = blendState;
            DepthStencilStateDescription = depthStencilStateDescription;
            RasterizerState = rasterizerState;
            PrimitiveTopology = primitiveTopology;
            ShaderSet = shaderSet;
        }
    }
}
