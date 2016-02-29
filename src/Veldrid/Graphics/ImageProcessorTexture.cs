using ImageProcessor;
using System.IO;
using System;

namespace Veldrid.Graphics
{
    public class ImageProcessorTexture : Texture
    {
        private readonly Image _image;

        public float[] Pixels => _image.Pixels;

        public int Width => _image.Width;
        public int Height => _image.Height;

        public PixelFormat Format => PixelFormat.R32_G32_B32_A32_Float;

        public int PixelSizeInBytes => sizeof(float) * 4;

        public ImageProcessorTexture(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                _image = new Image(fs);
            }
        }
    }
}
