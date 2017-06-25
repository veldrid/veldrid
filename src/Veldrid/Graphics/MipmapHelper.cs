using ImageSharp;
using ImageSharp.PixelFormats;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics
{
    public static class MipmapHelper
    {
        public static Image<T>[] GenerateMipmaps<T>(Image<T> image) where T : struct, IPixel<T>
        {
            int mipLevelCount = ComputeMipLevels(image.Width, image.Height);
            Image<T>[] mipLevels = new Image<T>[mipLevelCount];
            int i = 0;
            while (image.Width != 1 && image.Height != 1)
            {
                int newWidth = Math.Max(1, image.Width / 2);
                int newHeight = Math.Max(1, image.Height / 2);
                Image<T> newImage = image.Resize(newWidth, newHeight);
                Debug.Assert(i < mipLevelCount);
                mipLevels[i] = newImage;

                i++;
                image = newImage;
            }

            Debug.Assert(i == mipLevelCount);

            return mipLevels;
        }

        public static int ComputeMipLevels(int width, int height)
        {
            return 1 + (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
        }
    }
}
