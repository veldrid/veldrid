using ImageProcessorCore;
using System.IO;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public class PngLoader : AssetLoader<ImageProcessorTexture>
    {
        public override ImageProcessorTexture Load(Stream s)
        {
            return new ImageProcessorTexture(new Image(s));
        }
    }
}
