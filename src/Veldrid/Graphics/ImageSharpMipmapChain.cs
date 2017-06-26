using ImageSharp;
using System;

namespace Veldrid.Graphics
{
    public class ImageSharpMipmapChain : TextureData
    {
        public Image<Rgba32>[] Images { get; }

        public int Width => Images[0].Width;

        public int Height => Images[0].Height;

        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UInt;

        public int PixelSizeInBytes => sizeof(byte) * 4;

        public int MipLevels => Images.Length;

        public ImageSharpMipmapChain(string path)
            : this(Image.Load<Rgba32>(path))
        { }

        public ImageSharpMipmapChain(Image<Rgba32> baseImage)
        {
            Images = MipmapHelper.GenerateMipmaps(baseImage);
        }

        public void AcceptPixelData(PixelDataProvider pixelDataProvider)
        {
            throw new NotImplementedException();
        }

        public unsafe DeviceTexture2D CreateDeviceTexture(ResourceFactory factory)
        {
            DeviceTexture2D tex = factory.CreateTexture(MipLevels, Width, Height, PixelSizeInBytes, Format);
            for (int level = 0; level < MipLevels; level++)
            {
                Image<Rgba32> image = Images[level];
                fixed (void* pin = &image.Pixels.DangerousGetPinnableReference())
                {
                    tex.SetTextureData(level, 0, 0, image.Width, image.Height, (IntPtr)pin, PixelSizeInBytes * Width * Height);
                }
            }

            return tex;
        }
    }
}
