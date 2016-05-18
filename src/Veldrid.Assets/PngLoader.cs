using ImageProcessorCore;
using System.IO;
using Veldrid.Graphics;
using System;

namespace Veldrid.Assets
{
    public class PngLoader : AssetLoader<ImageProcessorTexture>
    {
        public override string FileExtension => "png";

        public override ImageProcessorTexture Load(Stream s)
        {
            return new ImageProcessorTexture(new Image(s));
        }
    }
}
