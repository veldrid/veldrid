using Veldrid.Graphics;
using System;

namespace Veldrid.Assets
{
    public abstract class TextureAsset : AssetDefinition<MaterialTextureInputElement>
    {
        public string Name { get; set; }
    }

    public class Texture2DAsset : TextureAsset
    {
        public AssetRef<ImageProcessorTexture> ImageSource { get; set; }

        public override MaterialTextureInputElement Create(AssetDatabase ad)
        {
            ImageProcessorTexture texture = ad.LoadAsset(ImageSource);
            return new TextureDataInputElement(Name, texture);
        }
    }

    public class CubeMapTextureAsset : TextureAsset
    {
        public AssetRef<ImageProcessorTexture> Image0 { get; private set; }
        public AssetRef<ImageProcessorTexture> Image1 { get; private set; }
        public AssetRef<ImageProcessorTexture> Image2 { get; private set; }
        public AssetRef<ImageProcessorTexture> Image3 { get; private set; }
        public AssetRef<ImageProcessorTexture> Image4 { get; private set; }
        public AssetRef<ImageProcessorTexture> Image5 { get; private set; }


        public override MaterialTextureInputElement Create(AssetDatabase ad)
        {
            throw new NotImplementedException();
        }
    }
}
