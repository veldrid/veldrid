namespace Veldrid.Graphics.Direct3D
{
    public class D3DShaderSet : ShaderSet
    {
        public D3DVertexInputLayout InputLayout { get; }

        public D3DVertexShader VertexShader { get; }

        public D3DGeometryShader GeometryShader { get; }

        public D3DFragmentShader FragmentShader { get; }

        public D3DShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader geometryShader, Shader fragmentShader)
        {
            InputLayout = (D3DVertexInputLayout)inputLayout;
            VertexShader = (D3DVertexShader)vertexShader;
            GeometryShader =(D3DGeometryShader)geometryShader;
            FragmentShader = (D3DFragmentShader)fragmentShader;
        }

        VertexInputLayout ShaderSet.InputLayout => InputLayout;
        Shader ShaderSet.VertexShader => VertexShader;
        Shader ShaderSet.GeometryShader => GeometryShader;
        Shader ShaderSet.FragmentShader => FragmentShader;

        public void Dispose()
        {
            InputLayout.Dispose();
            VertexShader.Dispose();
            GeometryShader?.Dispose();
            FragmentShader.Dispose();
        }
    }
}