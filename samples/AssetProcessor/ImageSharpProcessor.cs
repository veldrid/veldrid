using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Veldrid;
using AssetPrimitives;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AssetProcessor
{
    public class ImageSharpProcessor : BinaryAssetProcessor<ProcessedTexture>
    {
        public override async Task<ProcessedTexture> ProcessT(Stream stream, string extension)
        {
            return await Task.Run(() =>
            {
                Image<Rgba32> image = Image.Load(stream);
                Image<Rgba32>[] mipmaps = GenerateMipmaps(image, out int totalSize);

                byte[] allTexData = new byte[totalSize];
                long offset = 0;
                unsafe
                {
                    fixed (byte* allTexDataPtr = allTexData)
                    {
                        foreach (Image<Rgba32> mipmap in mipmaps)
                        {
                            long mipSize = mipmap.Width * mipmap.Height * sizeof(Rgba32);
                            fixed (Rgba32* pixelPtr = &MemoryMarshal.GetReference(mipmap.GetPixelSpan()))
                            {
                                Buffer.MemoryCopy(pixelPtr, allTexDataPtr + offset, mipSize, mipSize);
                            }

                            offset += mipSize;
                        }
                    }
                }

                ProcessedTexture texData = new ProcessedTexture(
                        PixelFormat.R8_G8_B8_A8_UNorm_SRgb, TextureType.Texture2D,
                        (uint)image.Width, (uint)image.Height, 1,
                        (uint)mipmaps.Length, 1,
                        allTexData);
                return texData;
            });
        }

        private static readonly IResampler s_resampler = new Lanczos3Resampler();

        private static Image<T>[] GenerateMipmaps<T>(Image<T> baseImage, out int totalSize) where T : struct, IPixel<T>
        {
            int mipLevelCount = ComputeMipLevels(baseImage.Width, baseImage.Height);
            Image<T>[] mipLevels = new Image<T>[mipLevelCount];
            mipLevels[0] = baseImage;
            totalSize = baseImage.Width * baseImage.Height * Unsafe.SizeOf<T>();
            int i = 1;

            int currentWidth = baseImage.Width;
            int currentHeight = baseImage.Height;
            while (currentWidth != 1 || currentHeight != 1)
            {
                int newWidth = Math.Max(1, currentWidth / 2);
                int newHeight = Math.Max(1, currentHeight / 2);
                Image<T> newImage = baseImage.Clone(context => context.Resize(newWidth, newHeight, s_resampler));
                Debug.Assert(i < mipLevelCount);
                mipLevels[i] = newImage;

                totalSize += newWidth * newHeight * Unsafe.SizeOf<T>();
                i++;
                currentWidth = newWidth;
                currentHeight = newHeight;
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
