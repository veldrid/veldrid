using System;

namespace Veldrid.SampleGallery
{
    public interface IGalleryDriver
    {
        uint Width { get; }
        uint Height { get; }
        GraphicsDevice Device { get; }
        Swapchain MainSwapchain { get; }

        event Action Resized;
        event Action<double, InputSnapshot> Update;
        event Action<double> Render;
    }
}
