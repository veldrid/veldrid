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
        bool SupportsImGui { get; }

        InputStateView GetInputState();

        event Action Resized;
        event Action<double> Update;
        event Func<double, CommandBuffer[]> Render;
    }
}
