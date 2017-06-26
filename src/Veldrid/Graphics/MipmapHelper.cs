using ImageSharp;
using ImageSharp.PixelFormats;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics
{
    public static class MipmapHelper
    {
        public static Image<T>[] GenerateMipmaps<T>(Image<T> baseImage) where T : struct, IPixel<T>
        {
            int mipLevelCount = ComputeMipLevels(baseImage.Width, baseImage.Height);
            Image<T>[] mipLevels = new Image<T>[mipLevelCount];
            mipLevels[0] = baseImage;
            int i = 1;
            while (baseImage.Width != 1 || baseImage.Height != 1)
            {
                int newWidth = Math.Max(1, baseImage.Width / 2);
                int newHeight = Math.Max(1, baseImage.Height / 2);
                Image<T> newImage = new Image<T>(baseImage).Resize(newWidth, newHeight);
                Debug.Assert(i < mipLevelCount);
                mipLevels[i] = newImage;

                i++;
                baseImage = newImage;
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
