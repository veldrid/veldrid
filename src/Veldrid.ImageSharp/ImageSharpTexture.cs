using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Diagnostics;

namespace Veldrid.ImageSharp
{
    public class ImageSharpTexture
    {
        /// <summary>
        /// An array of images, each a single element in the mipmap chain.
        /// The first element is the largest, most detailed level, and each subsequent element
        /// is half its size, down to 1x1 pixel.
        /// </summary>
        public Image<Rgba32>[] Images { get; }

        /// <summary>
        /// The width of the largest image in the chain.
        /// </summary>
        public uint Width => (uint)Images[0].Width;

        /// <summary>
        /// The height of the largest image in the chain.
        /// </summary>
        public uint Height => (uint)Images[0].Height;

        /// <summary>
        /// The pixel format of all images.
        /// </summary>
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UNorm;

        /// <summary>
        /// The size of each pixel, in bytes.
        /// </summary>
        public uint PixelSizeInBytes => sizeof(byte) * 4;

        /// <summary>
        /// The number of levels in the mipmap chain. This is equal to the length of the Images array.
        /// </summary>
        public uint MipLevels => (uint)Images.Length;

        public ImageSharpTexture(string path) : this(Image.Load(path), true) { }
        public ImageSharpTexture(string path, bool mipmap) : this(Image.Load(path), mipmap) { }
        public ImageSharpTexture(Image<Rgba32> image, bool mipmap = true)
        {
            if (mipmap)
            {
                Images = GenerateMipmaps(image);
            }
            else
            {
                Images = new Image<Rgba32>[] { image };
            }
        }

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory factory)
        {
            return CreateTextureViaStaging(gd, factory);
        }

        private unsafe Texture CreateTextureViaStaging(GraphicsDevice gd, ResourceFactory factory)
        {
            Texture staging = factory.CreateTexture(
                new TextureDescription(Width, Height, 1, MipLevels, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            Texture ret = factory.CreateTexture(
                new TextureDescription(Width, Height, 1, MipLevels, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));

            CommandList cl = gd.ResourceFactory.CreateCommandList();
            cl.Begin();
            for (int level = 0; level < MipLevels; level++)
            {
                Image<Rgba32> image = Images[level];
                fixed (void* pin = &image.DangerousGetPinnableReferenceToPixelBuffer())
                {
                    gd.UpdateTexture(
                        staging,
                        (IntPtr)pin,
                        (uint)(PixelSizeInBytes * image.Width * image.Height),
                        0, 0, 0,
                        (uint)image.Width, (uint)image.Height, 1,
                        (uint)level,
                        0);
                    cl.CopyTexture(
                        staging, 0, 0, 0, (uint)level, 0,
                        ret, 0, 0, 0, (uint)level, 0,
                        (uint)image.Width, (uint)image.Height, 1, 1);
                }
            }
            cl.End();

            gd.ExecuteCommands(cl);
            cl.Dispose();
            staging.Dispose();

            return ret;
        }

        private unsafe Texture CreateTextureViaUpdate(GraphicsDevice gd, ResourceFactory factory)
        {
            Texture tex = factory.CreateTexture(new TextureDescription(Width, Height, 1, MipLevels, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            for (int level = 0; level < MipLevels; level++)
            {
                Image<Rgba32> image = Images[level];
                fixed (void* pin = &image.DangerousGetPinnableReferenceToPixelBuffer())
                {
                    gd.UpdateTexture(
                        tex,
                        (IntPtr)pin,
                        (uint)(PixelSizeInBytes * image.Width * image.Height),
                        0,
                        0,
                        0,
                        (uint)image.Width,
                        (uint)image.Height,
                        1,
                        (uint)level,
                        0);
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

            int currentWidth = baseImage.Width;
            int currentHeight = baseImage.Height;
            while (currentWidth != 1 || currentHeight != 1)
            {
                int newWidth = Math.Max(1, currentWidth / 2);
                int newHeight = Math.Max(1, currentHeight / 2);
                Image<T> newImage = baseImage.Clone(context => context.Resize(newWidth, newHeight, s_resampler));
                Debug.Assert(i < mipLevelCount);
                mipLevels[i] = newImage;

                i++;
                currentWidth = newWidth;
                currentHeight = newHeight;
            }

            Debug.Assert(i == mipLevelCount);

            return mipLevels;
        }
    }
}
