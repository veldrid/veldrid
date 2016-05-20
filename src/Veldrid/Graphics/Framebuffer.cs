using System;

namespace Veldrid.Graphics
{
    public interface Framebuffer : RenderStateModifier, IDisposable
    {
        DeviceTexture2D ColorTexture { get; set; }
        DeviceTexture2D DepthTexture { get; set; }

        int Width { get; }
        int Height { get; }
    }
}