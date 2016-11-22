using ImageSharp;
using System.IO;
using Veldrid.Graphics;
using System;

namespace Veldrid.Assets
{
    public class PngLoader : ConcreteLoader<ImageSharpTexture>
    {
        public override string FileExtension => "png";

        public override ImageSharpTexture Load(Stream s)
        {
            return new ImageSharpTexture(new Image(s));
        }
    }
}
