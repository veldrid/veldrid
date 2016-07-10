using System;
using System.IO;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public static class Textures
    {
        public static readonly ImageProcessorTexture CubeTexture = new ImageProcessorTexture(Path.Combine(AppContext.BaseDirectory, "Assets/Textures/CubeTexture.png"));
        public static readonly ImageProcessorTexture WoodTexture = new ImageProcessorTexture(Path.Combine(AppContext.BaseDirectory, "Assets/Textures/Wood.png"));
        public static readonly TextureData PureWhiteTexture = LoadWhiteTextureData();

        private static TextureData LoadWhiteTextureData()
        {
            var texture = new RawTextureDataArray<RgbaFloat>(1, 1, RgbaFloat.SizeInBytes, PixelFormat.R32_G32_B32_A32_Float);
            texture.PixelData[0] = RgbaFloat.White;
            return texture;
        }
    }
}
