using System;

namespace Veldrid.SampleGallery
{
    public interface IGalleryDriver
    {
        uint Width { get; }
        uint Height { get; }
        GraphicsDevice Device { get; }
        Swapchain MainSwapchain { get; }
        uint FrameIndex { get; }
        uint BufferCount { get; }

        InputStateView GetInputState();

        event Action Resized;
        event Action<double> Update;
        event Action<double, CommandBuffer> Render;
    }
}
