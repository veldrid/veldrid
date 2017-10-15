namespace Vd2
{
    public struct PipelineDescription
    {
        public BlendStateDescription BlendState;
        public DepthStencilStateDescription DepthStencilState;
        public RasterizerStateDescription RasterizerState;
        public ShaderSetDescription ShaderSet;
        public PrimitiveTopology PrimitiveTopology;
    }
}
