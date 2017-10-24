using System;
using System.Collections.Generic;

namespace Vd2
{
    public abstract class Framebuffer : IDisposable
    {
        public virtual Texture2D DepthTexture { get; protected set; }
        public virtual IReadOnlyList<Texture2D> ColorTextures { get; protected set; }
        public virtual OutputDescription OutputDescription { get; protected set; }

        public virtual uint Width { get; }
        public virtual uint Height { get; }

        internal Framebuffer(Texture2D depthTexture, IReadOnlyList<Texture2D> colorTextures)
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
