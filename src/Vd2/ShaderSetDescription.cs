namespace Vd2
{
    public struct ShaderSetDescription
    {
        public VertexLayoutDescription[] VertexLayouts;
        public ShaderStageDescription[] ShaderStages;

        public ShaderSetDescription(VertexLayoutDescription[] vertexLayouts, ShaderStageDescription[] shaderStages)
        {
            VertexLayouts = vertexLayouts;
            ShaderStages = shaderStages;
        }
    }
}
