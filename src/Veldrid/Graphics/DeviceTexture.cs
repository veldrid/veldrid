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

        int MipLevels { get; }
    }

    public interface DeviceTexture2D : DeviceTexture
    {
        /// <summary>
        /// Copies texture data into the device's texture storage.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="data"></param>
        /// <param name="dataSizeInBytes"></param>
        void SetTextureData(int mipLevel, int x, int y, int width, int height, IntPtr data, int dataSizeInBytes);

        /// <summary>
        /// Retrieves texture data from the DeviceTexture2D into a CPU-side buffer.
        /// </summary>
        void GetTextureData(int mipLevel, IntPtr destination, int storageSizeInBytes);

        /// <summary>
        /// Retrieves texture data from the DeviceTexture2D into a CPU-side buffer.
        /// </summary>
        void GetTextureData<T>(int mipLevel, T[] destination) where T : struct;
    }
}
