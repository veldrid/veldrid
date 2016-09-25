using System;

namespace Veldrid.Graphics
{
    public interface ShaderSet : IDisposable
    {
        VertexInputLayout InputLayout { get; }
        Shader VertexShader { get; }
        Shader GeometryShader { get; }
        Shader FragmentShader { get; }
    }
}
