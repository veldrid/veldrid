using System;
using Veldrid.Graphics.Direct3D;

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
    }

    public class RawTextureData<T> : TextureData where T : struct
    {
        public int Width { get; }
        public int Height { get; }

        public T[] PixelData { get; }

        public PixelFormat Format { get; }

        public int PixelSizeInBytes { get; }

        public DeviceTexture CreateDeviceTexture(DeviceTextureCreator producer)
        {
            return producer.CreateTexture(PixelData, Width, Height, PixelSizeInBytes, Format);
        }

        public void AcceptPixelData(PixelDataProvider pixelDataProvider)
        {
            pixelDataProvider.SetPixelData(PixelData, Width, Height, PixelSizeInBytes);
        }
    }
}
