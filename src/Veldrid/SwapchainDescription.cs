using System;

namespace Veldrid
{
    public struct SwapchainDescription : IEquatable<SwapchainDescription>
    {
        public SwapchainSource Source;

        // TODO-SWAPCHAIN: Can these two be obtained via the SwapchainSource?
        public uint Width;
        public uint Height;

        public PixelFormat? DepthFormat;
        public bool SyncToVerticalBlank;

        public SwapchainDescription(SwapchainSource source, uint width, uint height, PixelFormat? depthFormat, bool syncToVerticalBlank)
        {
            Source = source;
            Width = width;
            Height = height;
            DepthFormat = depthFormat;
            SyncToVerticalBlank = syncToVerticalBlank;
        }

        public bool Equals(SwapchainDescription other)
        {
            return Source.Equals(other.Source)
                && DepthFormat == other.DepthFormat
                && SyncToVerticalBlank.Equals(other.SyncToVerticalBlank);
        }
    }
}
