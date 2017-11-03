using System;
using System.Collections.Generic;

namespace Veldrid
{
    public abstract class Framebuffer : DeviceResource, IDisposable
    {
        public virtual Texture DepthTexture { get; }
        public virtual IReadOnlyList<Texture> ColorTextures { get; }
        public virtual OutputDescription OutputDescription { get; }

        public virtual uint Width { get; }
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

        public abstract void Dispose();
    }
}
