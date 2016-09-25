namespace Veldrid.Graphics.Direct3D
{
    public class D3DShaderSet : ShaderSet
    {
        public VertexInputLayout InputLayout { get; }

        public Shader VertexShader { get; }

        public Shader GeometryShader { get; }

        public Shader FragmentShader { get; }

        public D3DShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader geometryShader, Shader fragmentShader)
        {
            InputLayout = inputLayout;
            VertexShader = vertexShader;
            GeometryShader = geometryShader;
            FragmentShader = fragmentShader;
        }

        public void Dispose()
        {
            InputLayout.Dispose();
            VertexShader.Dispose();
            GeometryShader?.Dispose();
            FragmentShader.Dispose();
        }
    }
}