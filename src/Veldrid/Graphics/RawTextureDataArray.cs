namespace Veldrid.Graphics
{
    public class RawTextureDataArray<T> : RawTextureDataBase where T : struct
    {
        public T[] PixelData { get; }

        public override void AcceptPixelData(PixelDataProvider pixelDataProvider)
        {
            pixelDataProvider.SetPixelData(PixelData, Width, Height, PixelSizeInBytes);
        }

        public override DeviceTexture2D CreateDeviceTexture(ResourceFactory factory)
        {
            return factory.CreateTexture(PixelData, Width, Height, PixelSizeInBytes, Format);
        }

        public RawTextureDataArray(int width, int height, int pixelSizeInBytes, PixelFormat format)
            : this(new T[width * height], width, height, pixelSizeInBytes, format)
        { }

        public RawTextureDataArray(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
            : base(width, height, pixelSizeInBytes, format)
        {
            PixelData = pixelData;
        }

        public static RawTextureDataArray<T> FromSingleColor(T color)
        {
            return new RawTextureDataArray<T>(new[] { color }, 1, 1, RgbaFloat.SizeInBytes, PixelFormat.R32_G32_B32_A32_Float);
        }
    }
}
