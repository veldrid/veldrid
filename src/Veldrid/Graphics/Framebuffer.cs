using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A device-side buffer, containing a color and/or depth target which can be rendered to.
    /// </summary>
    public interface Framebuffer : IDisposable
    {
        /// <summary>
        /// The first texture into which color data is rendered when this Framebuffer is bound.
        /// Note: multiple color textures may be attached to the framebuffer
        /// </summary>
        DeviceTexture2D ColorTexture { get; set; }

        /// <summary>
        /// The texture into which depth data is rendered when this Framebuffer is bound.
        /// </summary>
        DeviceTexture2D DepthTexture { get; set; }

        /// <summary>
        /// Gets the color texture attached at the given index.
        /// </summary>
        /// <param name="index">The index to retrieve.</param>
        /// <returns>The <see cref="DeviceTexture2D"/> attached at the given index.</returns>
        DeviceTexture2D GetColorTexture(int index);

        /// <summary>
        /// Attaches a color texture to be drawn into at the given slot.
        /// </summary>
        /// <param name="index">The index to attach into.</param>
        /// <param name="texture">The <see cref="DeviceTexture2D"/> to attach.</param>
        void AttachColorTexture(int index, DeviceTexture2D texture);

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