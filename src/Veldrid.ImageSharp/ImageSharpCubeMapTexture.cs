using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.ImageSharp
{
    public class ImageSharpCubeMapTexture
    {
    
        /// <summary>
        /// An array of images, each face of a cubemap.
        /// </summary>
        public Image<Rgba32>[] CubeMapTextures { get; }

        /// <summary>
        /// The width of a cubemap texture.
        /// </summary>
        public uint Width => (uint)CubeMapTextures[0].Width;

        /// <summary>
        /// The height of a cubemap texture.
        /// </summary>
        public uint Height => (uint)CubeMapTextures[0].Height;

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
        private const int FRONT = 0;
        private const int BACK = 1;
        private const int LEFT = 2;
        private const int RIGHT = 3;
        private const int TOP = 4;
        private const int BOTTOM = 5;

        public ImageSharpCubeMapTexture(string frontPath,
                                        string backPath,
                                        string leftPath,
                                        string rightPath,
                                        string topPath,
                                        string bottomPath) 
                                        : this(Image.Load(frontPath), 
                                               Image.Load(backPath),
                                               Image.Load(leftPath),
                                               Image.Load(rightPath),
                                               Image.Load(topPath),
                                               Image.Load(bottomPath)) { }

        public ImageSharpCubeMapTexture(Image<Rgba32> front,
                                        Image<Rgba32> back,
                                        Image<Rgba32> left,
                                        Image<Rgba32> right,
                                        Image<Rgba32> top,
                                        Image<Rgba32> bottom)
        {
            CubeMapTextures = new Image<Rgba32>[] { front, back, left, right, top, bottom };        
        }

        public unsafe Texture CreateCubeMapTextures(GraphicsDevice gd, ResourceFactory factory)
        {
            Texture cubeMapTexture;

            fixed (Rgba32* frontPin = &MemoryMarshal.GetReference(CubeMapTextures[FRONT].GetPixelSpan()))
            fixed (Rgba32* backPin = &MemoryMarshal.GetReference(CubeMapTextures[BACK].GetPixelSpan()))
            fixed (Rgba32* leftPin = &MemoryMarshal.GetReference(CubeMapTextures[LEFT].GetPixelSpan()))
            fixed (Rgba32* rightPin = &MemoryMarshal.GetReference(CubeMapTextures[RIGHT].GetPixelSpan()))
            fixed (Rgba32* topPin = &MemoryMarshal.GetReference(CubeMapTextures[TOP].GetPixelSpan()))
            fixed (Rgba32* bottomPin = &MemoryMarshal.GetReference(CubeMapTextures[BOTTOM].GetPixelSpan()))
            {
                cubeMapTexture = factory.CreateTexture(TextureDescription.Texture2D(
                    Width,
                    Height,
                    1,
                    1,
                    Format,
                    TextureUsage.Sampled | TextureUsage.Cubemap));

                uint faceSize = (uint)(Width * Height * PixelSizeInBytes);
                gd.UpdateTexture(cubeMapTexture, (IntPtr)rightPin, faceSize, 0, 0, 0, Width, Height, 1, 0, 0);
                gd.UpdateTexture(cubeMapTexture, (IntPtr)leftPin, faceSize, 0, 0, 0, Width, Height, 1, 0, 1);
                gd.UpdateTexture(cubeMapTexture, (IntPtr)topPin, faceSize, 0, 0, 0, Width, Height, 1, 0, 2);
                gd.UpdateTexture(cubeMapTexture, (IntPtr)bottomPin, faceSize, 0, 0, 0, Width, Height, 1, 0, 3);
                gd.UpdateTexture(cubeMapTexture, (IntPtr)backPin, faceSize, 0, 0, 0, Width, Height, 1, 0, 4);
                gd.UpdateTexture(cubeMapTexture, (IntPtr)frontPin, faceSize, 0, 0, 0, Width, Height, 1, 0, 5);

            }

            return cubeMapTexture;

        }

    }
}

