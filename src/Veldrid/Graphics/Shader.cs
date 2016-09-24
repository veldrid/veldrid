using System;

namespace Veldrid.Graphics
{
    public interface Shader : IDisposable
    {
        ShaderType Type { get; }
    }
}
