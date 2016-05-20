using ImageProcessorCore;
using System.IO;
using System;

namespace Veldrid.Graphics
{
    public class ImageProcessorTexture : TextureData
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

        public DeviceTexture2D CreateDeviceTexture(DeviceTextureCreator producer)
        {
            return producer.CreateTexture(Pixels, Width, Height, PixelSizeInBytes, Format);
        }

        public void AcceptPixelData(PixelDataProvider pixelDataProvider)
        {
            pixelDataProvider.SetPixelData(Pixels, Width, Height, PixelSizeInBytes);
        }
    }
}
