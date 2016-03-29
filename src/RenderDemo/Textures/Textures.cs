using System;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public static class Textures
    {
        public static readonly ImageProcessorTexture CubeTexture = new ImageProcessorTexture(AppContext.BaseDirectory + "/Textures/CubeTexture.png");
        public static readonly ImageProcessorTexture WoodTexture = new ImageProcessorTexture(AppContext.BaseDirectory + "/Textures/Wood.png");
        public static readonly TextureData PureWhiteTexture = LoadWhiteTextureData();

        private static TextureData LoadWhiteTextureData()
        {
            var texture = new RawTextureDataArray<RgbaFloat>(1, 1, RgbaFloat.SizeInBytes, Graphics.PixelFormat.R32_G32_B32_A32_Float);
            texture.PixelData[0] = RgbaFloat.White;
            return texture;
        }
    }
}
