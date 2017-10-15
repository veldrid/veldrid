using System.Collections.Generic;

namespace Vd2
{
    public abstract class Framebuffer
    {
        public Texture DepthTexture { get; }
        public IReadOnlyList<Texture> ColorTextures { get; }

        public Framebuffer(Texture depthTexture, IReadOnlyList<Texture> colorTextures)
        {
            ColorTextures = colorTextures;
            DepthTexture = depthTexture;
        }
    }
}
