using System;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public abstract class TextureAsset : AssetRef<DeviceTexture>
    {
        public string Name { get; set; }

        public abstract DeviceTexture Create(RenderContext rc, AssetDatabase ad);
    }

    public class Texture2DAsset
    {
    }

    public class CubeMapTextureAsset : TextureAsset
    {
        public string Image0 { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string Image4 { get; set; }
        public string Image5 { get; set; }

        public override DeviceTexture Create(RenderContext rc, AssetDatabase ad)
        {
            ad.Load<Texture2DAsset>(Image0);

            rc.ResourceFactory.CreateCubeMapTexture()
        }
    }
}
