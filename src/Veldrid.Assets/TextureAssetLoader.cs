using ImageProcessorCore;
using System;
using System.IO;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    internal class TextureAssetLoader : AssetLoader<DeviceTexture, Texture2DAsset>
    {
        public Type TypeLoaded => typeof(TextureAsset);

        public Texture2DAsset Load(Stream s, string name)
        {
            Image i = new Image(s);

        }

        object AssetLoader.Load(Stream s, string name)
        {
            throw new NotImplementedException();
        }
    }
}