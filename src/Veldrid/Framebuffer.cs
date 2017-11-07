using System;
using System.Collections.Generic;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to control which color and depth textures are rendered to.
    /// </summary>
    public abstract class Framebuffer : DeviceResource, IDisposable
    {
        /// <summary>
        /// Gets the depth texture associated with this instance. May be null if no depth texture is used.
        /// </summary>
        public virtual Texture DepthTexture { get; }

        /// <summary>
        /// Gets the collection of color textures associated with this instance. May be empty.
        /// </summary>
        public virtual IReadOnlyList<Texture> ColorTextures { get; }

        /// <summary>
        /// Gets an <see cref="Veldrid.OutputDescription"/> which describes the number and formats of the color targets in this instance.
        /// </summary>
        public virtual OutputDescription OutputDescription { get; }

        /// <summary>
        /// Gets the width of the <see cref="Framebuffer"/>.
        /// </summary>
        public virtual uint Width { get; }

        /// <summary>
        /// Gets the height of the <see cref="Framebuffer"/>.
        /// </summary>
        public virtual uint Height { get; }

        internal Framebuffer(Texture depthTexture, IReadOnlyList<Texture> colorTextures)
        {
            ColorTextures = colorTextures;
            DepthTexture = depthTexture;

            if (colorTextures.Count > 0)
            {
                Width = colorTextures[0].Width;
                Height = colorTextures[0].Height;
            }
            else if (depthTexture != null)
            {
                Width = depthTexture.Width;
                Height = depthTexture.Height;
            }

            OutputDescription = OutputDescription.CreateFromFramebuffer(this);
        }

        internal Framebuffer() { }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
