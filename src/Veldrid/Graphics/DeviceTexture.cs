using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// Represents a texture object owned by the graphics device.
    /// </summary>
    public interface DeviceTexture : IDisposable
    {
        /// <summary>
        /// The width of the texture. For cubemap textures, returns the width of each individual face.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the texture. For cubemap textures, returns the height of each individual face.
        /// </summary>
        int Height { get; }
    }

    public interface DeviceTexture2D : DeviceTexture
    {
        /// <summary>
        /// Copies the DeviceTexture's pixel data into a CPU-side Texture.
        /// </summary>
        /// <param name="textureData">The TextureData to copy the pixel data into.</param>
        void CopyTo(TextureData textureData);

        /// <summary>
        /// Copies texture data into the device's texture storage.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="data"></param>
        /// <param name="dataSizeInBytes"></param>
        void SetTextureData(int x, int y, int width, int height, IntPtr data, int dataSizeInBytes);
    }
}
