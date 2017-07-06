using ImageSharp;
using ImageSharp.PixelFormats;
using ImageSharp.Processing;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A TextureData providing a full set of mipmap textures, based on an original ImageSharp image.
    /// </summary>
    public class ImageSharpMipmapChain : TextureData
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
        public int Width => Images[0].Width;

        /// <summary>
        /// The height of the largest image in the chain.
        /// </summary>
        public int Height => Images[0].Height;

        /// <summary>
        /// The pixel format of all images.
        /// </summary>
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UInt;

        /// <summary>
        /// The size of each pixel, in bytes.
        /// </summary>
        public int PixelSizeInBytes => sizeof(byte) * 4;

        /// <summary>
        /// The number of levels in the mipmap chain. This is equal to the length of the Images array.
        /// </summary>
        public int MipLevels => Images.Length;

        /// <summary>
        /// Constructs a new ImageSharpMipmapChain from the file located at the given path.
        /// </summary>
        /// <param name="path">The path to the image file to use as the base image.</param>
        public ImageSharpMipmapChain(string path)
            : this(Image.Load<Rgba32>(path))
        { }

        /// <summary>
        /// Constructs a new ImageSharpMipmapChain from the given base image.
        /// </summary>
        /// <param name="baseImage">The base image. This is used as the highest-detail mipmap level.</param>
        public ImageSharpMipmapChain(Image<Rgba32> baseImage)
        {
            Images = GenerateMipmaps(baseImage);
        }

        public void AcceptPixelData(PixelDataProvider pixelDataProvider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs a mipmapped 2D device texture from this mipmap chain.
        /// </summary>
        /// <param name="factory">The ResourceFactory used to create device resources.</param>
        /// <returns>A mpimapped 2D device texture.</returns>
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
