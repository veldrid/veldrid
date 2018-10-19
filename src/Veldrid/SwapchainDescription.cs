using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="Swapchain"/>, for creation via a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct SwapchainDescription : IEquatable<SwapchainDescription>
    {
        /// <summary>
        /// The <see cref="SwapchainSource"/> which will be used as the target of rendering operations.
        /// This is a window-system-specific object which differs by platform.
        /// </summary>
        public SwapchainSource Source;
        /// <summary>
        /// The initial width of the Swapchain surface.
        /// </summary>
        public uint Width;
        /// <summary>
        /// The initial height of the Swapchain surface.
        /// </summary>
        public uint Height;
        /// <summary>
        /// The optional format of the depth target of the Swapchain's Framebuffer.
        /// If non-null, this must be a valid depth Texture format.
        /// If null, then no depth target will be created.
        /// </summary>
        public PixelFormat? DepthFormat;
        /// <summary>
        /// Indicates whether presentation of the Swapchain will be synchronized to the window system's vertical refresh rate.
        /// </summary>
        public bool SyncToVerticalBlank;
        /// <summary>
        /// Indicates whether the color target of the Swapchain will use an sRGB PixelFormat.
        /// </summary>
        public bool ColorSrgb;

        /// <summary>
        /// Constructs a new SwapchainDescription.
        /// </summary>
        /// <param name="source">The <see cref="SwapchainSource"/> which will be used as the target of rendering operations.
        /// This is a window-system-specific object which differs by platform.</param>
        /// <param name="width">The initial width of the Swapchain surface.</param>
        /// <param name="height">The initial height of the Swapchain surface.</param>
        /// <param name="depthFormat">The optional format of the depth target of the Swapchain's Framebuffer.
        /// If non-null, this must be a valid depth Texture format.
        /// If null, then no depth target will be created.</param>
        /// <param name="syncToVerticalBlank">Indicates whether presentation of the Swapchain will be synchronized to the window
        /// system's vertical refresh rate.</param>
        public SwapchainDescription(
            SwapchainSource source,
            uint width,
            uint height,
            PixelFormat? depthFormat,
            bool syncToVerticalBlank)
        {
            Source = source;
            Width = width;
            Height = height;
            DepthFormat = depthFormat;
            SyncToVerticalBlank = syncToVerticalBlank;
            ColorSrgb = false;
        }

        /// <summary>
        /// Constructs a new SwapchainDescription.
        /// </summary>
        /// <param name="source">The <see cref="SwapchainSource"/> which will be used as the target of rendering operations.
        /// This is a window-system-specific object which differs by platform.</param>
        /// <param name="width">The initial width of the Swapchain surface.</param>
        /// <param name="height">The initial height of the Swapchain surface.</param>
        /// <param name="depthFormat">The optional format of the depth target of the Swapchain's Framebuffer.
        /// If non-null, this must be a valid depth Texture format.
        /// If null, then no depth target will be created.</param>
        /// <param name="syncToVerticalBlank">Indicates whether presentation of the Swapchain will be synchronized to the window
        /// system's vertical refresh rate.</param>
        /// <param name="colorSrgb">Indicates whether the color target of the Swapchain will use an sRGB PixelFormat.</param>
        public SwapchainDescription(
            SwapchainSource source,
            uint width,
            uint height,
            PixelFormat? depthFormat,
            bool syncToVerticalBlank,
            bool colorSrgb)
        {
            Source = source;
            Width = width;
            Height = height;
            DepthFormat = depthFormat;
            SyncToVerticalBlank = syncToVerticalBlank;
            ColorSrgb = colorSrgb;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(SwapchainDescription other)
        {
            return Source.Equals(other.Source)
                && Width.Equals(other.Width)
                && Height.Equals(other.Height)
                && DepthFormat == other.DepthFormat
                && SyncToVerticalBlank.Equals(other.SyncToVerticalBlank)
                && ColorSrgb.Equals(other.ColorSrgb);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                Source.GetHashCode(),
                Width.GetHashCode(),
                Height.GetHashCode(),
                DepthFormat.GetHashCode(),
                SyncToVerticalBlank.GetHashCode(),
                ColorSrgb.GetHashCode());
        }
    }
}
