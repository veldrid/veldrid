using System;

namespace Veldrid.Graphics
{
    public interface Framebuffer : IDisposable
    {
        DeviceTexture2D ColorTexture { get; set; }
        DeviceTexture2D DepthTexture { get; set; }

        int Width { get; }
        int Height { get; }
    }
}