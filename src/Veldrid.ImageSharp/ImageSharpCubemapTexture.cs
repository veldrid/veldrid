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
        private const int POSITIVE_X = 0;
        private const int NEGATIVE_X = 1;
        private const int POSITIVE_Y = 2;
        private const int NEGATIVE_Y = 3;
        private const int POSITIVE_Z = 4;
        private const int NEGATIVE_Z = 5;

        public ImageSharpCubemapTexture(
            string negativeZPath,
            string positiveZPath,
            string negativeXPath,
            string positiveXPath,
            string positiveYPath,
            string negativeYPath) : this(
                Image.Load(negativeZPath), 
                Image.Load(positiveZPath),
                Image.Load(negativeXPath),
                Image.Load(positiveXPath),
                Image.Load(positiveYPath),
                Image.Load(negativeYPath)) { }

        public ImageSharpCubemapTexture(
            Image<Rgba32> negativeZ,
            Image<Rgba32> positiveZ,
            Image<Rgba32> negativeX,
            Image<Rgba32> positiveX,
            Image<Rgba32> positiveY,
            Image<Rgba32> negativeY)
        {
            CubemapTextures = new Image<Rgba32>[] { positiveX, negativeX, positiveY, negativeY, positiveZ, negativeZ };        
        }

        public unsafe Texture CreateCubemapTextures(GraphicsDevice gd, ResourceFactory factory)
        {
            Texture cubemapTexture;

            fixed (Rgba32* negativeZPin = &MemoryMarshal.GetReference(CubemapTextures[NEGATIVE_Z].GetPixelSpan()))
            fixed (Rgba32* positiveZPin = &MemoryMarshal.GetReference(CubemapTextures[POSITIVE_Z].GetPixelSpan()))
            fixed (Rgba32* negativeXPin = &MemoryMarshal.GetReference(CubemapTextures[NEGATIVE_X].GetPixelSpan()))
            fixed (Rgba32* positiveXPin = &MemoryMarshal.GetReference(CubemapTextures[POSITIVE_X].GetPixelSpan()))
            fixed (Rgba32* positiveYPin = &MemoryMarshal.GetReference(CubemapTextures[POSITIVE_Y].GetPixelSpan()))
            fixed (Rgba32* negativeYPin = &MemoryMarshal.GetReference(CubemapTextures[NEGATIVE_Y].GetPixelSpan()))
            {
                cubemapTexture = factory.CreateTexture(TextureDescription.Texture2D(
                    Width,
                    Height,
                    1,
                    1,
                    Format,
                    TextureUsage.Sampled | TextureUsage.Cubemap));

                uint faceSize = (uint)(Width * Height * PixelSizeInBytes);
                gd.UpdateTexture(cubemapTexture, (IntPtr)positiveXPin, faceSize, 0, 0, 0, Width, Height, 1, 0, POSITIVE_X);
                gd.UpdateTexture(cubemapTexture, (IntPtr)negativeXPin, faceSize, 0, 0, 0, Width, Height, 1, 0, NEGATIVE_X);
                gd.UpdateTexture(cubemapTexture, (IntPtr)positiveYPin, faceSize, 0, 0, 0, Width, Height, 1, 0, POSITIVE_Y);
                gd.UpdateTexture(cubemapTexture, (IntPtr)negativeYPin, faceSize, 0, 0, 0, Width, Height, 1, 0, NEGATIVE_Y);
                gd.UpdateTexture(cubemapTexture, (IntPtr)positiveZPin, faceSize, 0, 0, 0, Width, Height, 1, 0, POSITIVE_Z);
                gd.UpdateTexture(cubemapTexture, (IntPtr)negativeZPin, faceSize, 0, 0, 0, Width, Height, 1, 0, NEGATIVE_Z);
            }
            return cubemapTexture;
        }

    }
}

