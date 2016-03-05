using ImageProcessor;
using System.IO;
using System;

namespace Veldrid.Graphics
{
    public class ImageProcessorTexture : Texture
    {
        public Image Image { get; }

        public float[] Pixels => Image.Pixels;

        public int Width => Image.Width;
        public int Height => Image.Height;

        public PixelFormat Format => PixelFormat.R32_G32_B32_A32_Float;

        public int PixelSizeInBytes => sizeof(float) * 4;

        public ImageProcessorTexture(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                Image = new Image(fs);
            }
        }

        public ImageProcessorTexture(Image image)
        {
            Image = image;
        }

        public void SaveToFile(string path)
        {
            using (FileStream fs = File.OpenWrite(path))
            {
                Image.Save(fs);
            }
        }
    }
}
