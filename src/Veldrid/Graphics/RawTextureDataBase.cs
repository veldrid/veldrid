namespace Veldrid.Graphics
{
    public abstract class RawTextureDataBase : TextureData
    {
        public int Width { get; }

        public int Height { get; }

        public PixelFormat Format { get; }

        public int PixelSizeInBytes { get; }

        public int MipLevels => 1;

        public abstract DeviceTexture2D CreateDeviceTexture(ResourceFactory factory);

        public RawTextureDataBase(int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            Width = width;
            Height = height;
            PixelSizeInBytes = pixelSizeInBytes;
            Format = format;
        }
    }
}
