using ImageSharp;
using ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace Veldrid.Graphics
{
    public static class MipmapHelper
    {
        public static List<Image<T>> GenerateMipmaps<T>(Image<T> image) where T : struct, IPixel<T>
        {
            int mipLevelCount = ComputeMipLevels(image.Width, image.Height);
            List<Image<T>> mipLevels = new List<Image<T>>(mipLevelCount);
            while (image.Width != 1 && image.Height != 1)
            {
                int newWidth = Math.Max(1, image.Width / 2);
                int newHeight = Math.Max(1, image.Height / 2);
                Image<T> newImage = image.Resize(newWidth, newHeight);
                mipLevels.Add(newImage);
                image = newImage;
            }

            return mipLevels;
        }

        public static int ComputeMipLevels(int width, int height)
        {
            return 1 + (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
        }
    }
}
