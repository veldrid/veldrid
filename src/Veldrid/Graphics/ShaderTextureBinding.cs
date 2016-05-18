using System;

namespace Veldrid.Graphics
{
    public interface ShaderTextureBinding : IDisposable
    {
        DeviceTexture BoundTexture { get; }
    }
}
