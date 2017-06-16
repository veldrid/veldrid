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
        /// Constructs a GPU-side <see cref="DeviceTexture2D"/> from this <see cref="TextureData"/>.
        /// </summary>
        /// <param name="producer">The device texture producer.</param>
        /// <returns>A new GPU <see cref="DeviceTexture2D"/>.</returns>
        DeviceTexture2D CreateDeviceTexture(DeviceTextureCreator producer);

        /// <summary>
        /// Accepts pixel data from a provider.
        /// </summary>
        /// <param name="pixelDataProvider">The provider whose data is transferred.</param>
        void AcceptPixelData(PixelDataProvider pixelDataProvider);
    }

    public interface DeviceTextureCreator
    {
        DeviceTexture2D CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format) where T : struct;
        DeviceTexture2D CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeinBytes, PixelFormat format);
        CubemapTexture CreateCubemapTexture(
            IntPtr pixelsFront,
            IntPtr pixelsBack,
            IntPtr pixelsLeft,
            IntPtr pixelsRight,
            IntPtr pixelsTop,
            IntPtr pixelsBottom,
            int width, int height, int pixelSizeinBytes, PixelFormat format);
    }

    public interface PixelDataProvider
    {
        void SetPixelData<T>(T[] pixelData, int width, int height, int pixelSizeInBytes) where T : struct;
        void SetPixelData(IntPtr pixelData, int width, int height, int pixelSizeInBytes);
    }

    public abstract class RawTextureDataBase : TextureData
    {
        public int Width { get; }

        public int Height { get; }

        public PixelFormat Format { get; }

        public int PixelSizeInBytes { get; }

        public abstract DeviceTexture2D CreateDeviceTexture(DeviceTextureCreator producer);

        public abstract void AcceptPixelData(PixelDataProvider pixelDataProvider);

        public RawTextureDataBase(int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            Width = width;
            Height = height;
            PixelSizeInBytes = pixelSizeInBytes;
            Format = format;
        }
    }

    public class RawTextureDataArray<T> : RawTextureDataBase where T : struct
    {
        public T[] PixelData { get; }

        public override void AcceptPixelData(PixelDataProvider pixelDataProvider)
        {
            pixelDataProvider.SetPixelData(PixelData, Width, Height, PixelSizeInBytes);
        }

        public override DeviceTexture2D CreateDeviceTexture(DeviceTextureCreator producer)
        {
            return producer.CreateTexture(PixelData, Width, Height, PixelSizeInBytes, Format);
        }

        public RawTextureDataArray(int width, int height, int pixelSizeInBytes, PixelFormat format)
            : this(new T[width * height], width, height, pixelSizeInBytes, format)
        { }

        public RawTextureDataArray(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
            : base(width, height, pixelSizeInBytes, format)
        {
            PixelData = pixelData;
        }

        public static RawTextureDataArray<RgbaFloat> FromSingleColor(RgbaFloat color)
        {
            return new RawTextureDataArray<RgbaFloat>(new[] { color }, 1, 1, RgbaFloat.SizeInBytes, PixelFormat.R32_G32_B32_A32_Float);
        }
    }

    public class RawTextureDataPtr : RawTextureDataBase
    {
        public IntPtr PixelData { get; }

        public override void AcceptPixelData(PixelDataProvider pixelDataProvider)
        {
            pixelDataProvider.SetPixelData(PixelData, Width, Height, PixelSizeInBytes);
        }

        public override DeviceTexture2D CreateDeviceTexture(DeviceTextureCreator producer)
        {
            return producer.CreateTexture(PixelData, Width, Height, PixelSizeInBytes, Format);
        }

        public RawTextureDataPtr(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
            : base(width, height, pixelSizeInBytes, format)
        {
            PixelData = pixelData;
        }
    }
}
