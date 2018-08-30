using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.ImageSharp
{
    public class ImageSharpCubemapTexture
    {
        /// <summary>
        /// An array of images, each face of a cubemap.
        /// </summary>
        public Image<Rgba32>[] CubemapTextures { get; }

        /// <summary>
        /// The width of a cubemap texture.
        /// </summary>
        public uint Width => (uint)CubemapTextures[0].Width;

        /// <summary>
        /// The height of a cubemap texture.
        /// </summary>
        public uint Height => (uint)CubemapTextures[0].Height;

        /// <summary>
        /// The pixel format cubemap textures.
        /// </summary>
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UNorm;

        /// <summary>
        /// The size of each pixel, in bytes.
        /// </summary>
        public uint PixelSizeInBytes => sizeof(byte) * 4;

        /// <summary>
        /// Provides standardized access to the cubemap texture array
        /// </summary>
        private const int PositiveXArrayLayer = 0;
        private const int NegativeXArrayLayer = 1;
        private const int PostitiveYArrayLayer = 2;
        private const int NegativeYArrayLayer = 3;
        private const int PositiveZArrayLayer = 4;
        private const int NegativeZArrayLayer = 5;

        public ImageSharpCubemapTexture(
            string positiveXPath,
            string negativeXPath,
            string positiveYPath,
            string negativeYPath,
            string positiveZPath,
            string negativeZPath) : this(
                Image.Load(positiveXPath),
                Image.Load(negativeXPath),
                Image.Load(positiveYPath),
                Image.Load(negativeYPath),
                Image.Load(positiveZPath), 
                Image.Load(negativeZPath)) { }

        public ImageSharpCubemapTexture(
            Image<Rgba32> positiveX,
            Image<Rgba32> negativeX,
            Image<Rgba32> positiveY,
            Image<Rgba32> negativeY,
            Image<Rgba32> positiveZ,
            Image<Rgba32> negativeZ)
        {
            CubemapTextures = new Image<Rgba32>[] { positiveX, negativeX, positiveY, negativeY, positiveZ, negativeZ };        
        }

        public unsafe Texture CreateCubemapTextures(GraphicsDevice gd, ResourceFactory factory)
        {
            Texture cubemapTexture;

            fixed (Rgba32* positiveXPin = &MemoryMarshal.GetReference(CubemapTextures[PositiveXArrayLayer].GetPixelSpan()))
            fixed (Rgba32* negativeXPin = &MemoryMarshal.GetReference(CubemapTextures[NegativeXArrayLayer].GetPixelSpan()))
            fixed (Rgba32* positiveYPin = &MemoryMarshal.GetReference(CubemapTextures[PostitiveYArrayLayer].GetPixelSpan()))
            fixed (Rgba32* negativeYPin = &MemoryMarshal.GetReference(CubemapTextures[NegativeYArrayLayer].GetPixelSpan()))
            fixed (Rgba32* positiveZPin = &MemoryMarshal.GetReference(CubemapTextures[PositiveZArrayLayer].GetPixelSpan()))
            fixed (Rgba32* negativeZPin = &MemoryMarshal.GetReference(CubemapTextures[NegativeZArrayLayer].GetPixelSpan()))
            {
                cubemapTexture = factory.CreateTexture(TextureDescription.Texture2D(
                    Width,
                    Height,
                    1,
                    1,
                    Format,
                    TextureUsage.Sampled | TextureUsage.Cubemap));

                uint faceSize = (uint)(Width * Height * PixelSizeInBytes);
                gd.UpdateTexture(cubemapTexture, (IntPtr)positiveXPin, faceSize, 0, 0, 0, Width, Height, 1, 0, PositiveXArrayLayer);
                gd.UpdateTexture(cubemapTexture, (IntPtr)negativeXPin, faceSize, 0, 0, 0, Width, Height, 1, 0, NegativeXArrayLayer);
                gd.UpdateTexture(cubemapTexture, (IntPtr)positiveYPin, faceSize, 0, 0, 0, Width, Height, 1, 0, PostitiveYArrayLayer);
                gd.UpdateTexture(cubemapTexture, (IntPtr)negativeYPin, faceSize, 0, 0, 0, Width, Height, 1, 0, NegativeYArrayLayer);
                gd.UpdateTexture(cubemapTexture, (IntPtr)positiveZPin, faceSize, 0, 0, 0, Width, Height, 1, 0, PositiveZArrayLayer);
                gd.UpdateTexture(cubemapTexture, (IntPtr)negativeZPin, faceSize, 0, 0, 0, Width, Height, 1, 0, NegativeZArrayLayer);
            }
            return cubemapTexture;
        }

    }
}

