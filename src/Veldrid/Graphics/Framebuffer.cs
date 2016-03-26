using System;

namespace Veldrid.Graphics
{
    public interface Framebuffer : RenderStateModifier, IDisposable
    {
        DeviceTexture ColorTexture { get; }
        DeviceTexture DepthTexture { get; }
    }
}