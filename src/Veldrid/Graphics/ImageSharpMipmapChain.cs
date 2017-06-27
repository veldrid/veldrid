using ImageSharp;
using ImageSharp.PixelFormats;
using ImageSharp.Processing;
using System;
using System.Diagnostics;

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
            Images = GenerateMipmaps(baseImage);
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

        private static readonly IResampler s_resampler = new Lanczos3Resampler();

        private static Image<T>[] GenerateMipmaps<T>(Image<T> baseImage) where T : struct, IPixel<T>
        {
            int mipLevelCount = MipmapHelper.ComputeMipLevels(baseImage.Width, baseImage.Height);
            Image<T>[] mipLevels = new Image<T>[mipLevelCount];
            mipLevels[0] = baseImage;
            int i = 1;
            while (baseImage.Width != 1 || baseImage.Height != 1)
            {
                int newWidth = Math.Max(1, baseImage.Width / 2);
                int newHeight = Math.Max(1, baseImage.Height / 2);
                Image<T> newImage = new Image<T>(baseImage).Resize(newWidth, newHeight, s_resampler);
                Debug.Assert(i < mipLevelCount);
                mipLevels[i] = newImage;

                i++;
                baseImage = newImage;
            }

            Debug.Assert(i == mipLevelCount);

            return mipLevels;
        }
    }
}
