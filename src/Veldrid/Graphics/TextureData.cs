using System;

namespace Veldrid.Graphics
{
    public interface TextureData
    {
        int Width { get; }
        int Height { get; }
        PixelFormat Format { get; }
        int PixelSizeInBytes { get; }
        DeviceTexture CreateDeviceTexture(DeviceTextureCreator producer);
        void AcceptPixelData(PixelDataProvider pixelDataProvider);
    }

    public interface DeviceTextureCreator
    {
        DeviceTexture CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format) where T : struct;
        DeviceTexture CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeinBytes, PixelFormat format);
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

        public abstract DeviceTexture CreateDeviceTexture(DeviceTextureCreator producer);

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

        public override DeviceTexture CreateDeviceTexture(DeviceTextureCreator producer)
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
    }

    public class RawTextureDataPtr : RawTextureDataBase
    {
        public IntPtr PixelData { get; }

        public override void AcceptPixelData(PixelDataProvider pixelDataProvider)
        {
            pixelDataProvider.SetPixelData(PixelData, Width, Height, PixelSizeInBytes);
        }

        public override DeviceTexture CreateDeviceTexture(DeviceTextureCreator producer)
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
