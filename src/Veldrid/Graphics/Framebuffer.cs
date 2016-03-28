using System;

namespace Veldrid.Graphics
{
    public interface Framebuffer : RenderStateModifier, IDisposable
    {
        DeviceTexture ColorTexture { get; set; }
        DeviceTexture DepthTexture { get; set; }
    }
}