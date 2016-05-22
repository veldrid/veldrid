using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A device-side buffer, containing a color and/or depth target which can be rendered to.
    /// </summary>
    public interface Framebuffer : IDisposable
    {
        /// <summary>
        /// The texture into which color data is rendered when this Framebuffer is bound.
        /// </summary>
        DeviceTexture2D ColorTexture { get; set; }
        /// <summary>
        /// The texture into which depth data is rendered when this Framebuffer is bound.
        /// </summary>
        DeviceTexture2D DepthTexture { get; set; }

        /// <summary>
        /// The width of the Framebuffer. This matches the width of the color and depth textures.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the Framebuffer. This matches the height of the color and depth textures.
        /// </summary>
        int Height { get; }
    }
}