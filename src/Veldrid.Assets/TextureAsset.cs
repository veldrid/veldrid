using System;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public abstract class TextureAsset : AssetDefinition<MaterialTextureInputElement>
    {
        public string Name { get; set; }
    }

    public class Texture2DAsset : TextureAsset
    {
        public AssetRef<ImageSharpTexture> ImageSource { get; set; }

        public override MaterialTextureInputElement Create(AssetDatabase ad)
        {
            ImageSharpTexture texture = ad.LoadAsset(ImageSource);
            return new TextureDataInputElement(Name, texture);
        }
    }

    public class CubeMapTextureAsset : TextureAsset
    {
        public AssetRef<ImageSharpTexture> Front { get; private set; }
        public AssetRef<ImageSharpTexture> Back { get; private set; }
        public AssetRef<ImageSharpTexture> Left { get; private set; }
        public AssetRef<ImageSharpTexture> Right { get; private set; }
        public AssetRef<ImageSharpTexture> Top { get; private set; }
        public AssetRef<ImageSharpTexture> Bottom { get; private set; }

        public override MaterialTextureInputElement Create(AssetDatabase ad)
        {
            return new CubemapTextureInputElement(
                Name,
                ad.LoadAsset(Front),
                ad.LoadAsset(Back),
                ad.LoadAsset(Left),
                ad.LoadAsset(Right),
                ad.LoadAsset(Top),
                ad.LoadAsset(Bottom));
        }
    }

    public class PlaceholderTexture : TextureAsset
    {
        public override MaterialTextureInputElement Create(AssetDatabase ad)
        {
            return new ManualTextureInput(Name);
        }
    }
}
