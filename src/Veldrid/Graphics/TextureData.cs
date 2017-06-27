using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A generic object which can be used to create GPU <see cref="DeviceTexture2D"/> objects.
    /// </summary>
    public interface TextureData
    {
        /// <summary>
        /// The width of the texture.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the texture.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The format of individual pixel data.
        /// </summary>
        PixelFormat Format { get; }

        /// <summary>
        /// The size of each individual pixel, in bytes.
        /// </summary>
        int PixelSizeInBytes { get; }

        /// <summary>
        /// The number of levels in the texture's mipmap chain.
        /// </summary>
        int MipLevels { get; }

        /// <summary>
        /// Constructs a GPU-side <see cref="DeviceTexture2D"/> from this <see cref="TextureData"/>.
        /// </summary>
        /// <param name="producer">The device texture producer.</param>
        /// <returns>A new GPU <see cref="DeviceTexture2D"/>.</returns>
        DeviceTexture2D CreateDeviceTexture(ResourceFactory factory);

        // TODO: This function does not understand mip levels.
        /// <summary>
        /// Accepts pixel data from a provider.
        /// </summary>
        /// <param name="pixelDataProvider">The provider whose data is transferred.</param>
        void AcceptPixelData(PixelDataProvider pixelDataProvider);
    }

    public interface PixelDataProvider
    {
        void SetPixelData<T>(T[] pixelData, int width, int height, int pixelSizeInBytes) where T : struct;
        void SetPixelData(IntPtr pixelData, int width, int height, int pixelSizeInBytes);
    }
}
