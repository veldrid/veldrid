namespace Veldrid.Graphics
{
    public class ShaderSet
    {
        public VertexInputLayout InputLayout { get; }
        public Shader VertexShader { get; }
        public Shader TesselationShader { get; }
        public Shader GeometryShader { get; }
        public Shader FragmentShader { get; }

        public ShaderSet(
            VertexInputLayout inputLayout,
            Shader vertexShader,
            Shader tesselationShader,
            Shader geometryShader,
            Shader fragmentShader)
        {
            InputLayout = inputLayout;
            VertexShader = vertexShader;
            TesselationShader = tesselationShader;
            GeometryShader = geometryShader;
            FragmentShader = fragmentShader;
        }
    }
}
