using ImageSharp;
using System.IO;
using Veldrid.Graphics;
using System;

namespace Veldrid.Assets
{
    public class PngLoader : ConcreteLoader<ImageSharpMipmapChain>
    {
        public override string FileExtension => "png";

        public override ImageSharpMipmapChain Load(Stream s)
        {
            return new ImageSharpMipmapChain(Image.Load(s));
        }
    }

    public class ImageSharpTextureLoader : ConcreteLoader<ImageSharpTexture>
    {
        public override string FileExtension => "png";

        public override ImageSharpTexture Load(Stream s)
        {
            return new ImageSharpTexture(Image.Load(s));
        }
    }
}
