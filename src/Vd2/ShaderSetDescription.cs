using System;

namespace Vd2
{
    public struct ShaderSetDescription : IEquatable<ShaderSetDescription>
    {
        public VertexLayoutDescription[] VertexLayouts;
        public ShaderStageDescription[] ShaderStages;

        public ShaderSetDescription(VertexLayoutDescription[] vertexLayouts, ShaderStageDescription[] shaderStages)
        {
            VertexLayouts = vertexLayouts;
            ShaderStages = shaderStages;
        }

        public bool Equals(ShaderSetDescription other)
        {
            return Util.ArrayEqualsEquatable(VertexLayouts, other.VertexLayouts);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(HashHelper.Array(VertexLayouts), HashHelper.Array(ShaderStages));
        }
    }
}
