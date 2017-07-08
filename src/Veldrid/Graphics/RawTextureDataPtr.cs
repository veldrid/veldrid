using System;

namespace Veldrid.Graphics
{
    public class RawTextureDataPtr : RawTextureDataBase
    {
        public IntPtr PixelData { get; }

        public override DeviceTexture2D CreateDeviceTexture(ResourceFactory factory)
        {
            return factory.CreateTexture(PixelData, Width, Height, PixelSizeInBytes, Format);
        }

        public RawTextureDataPtr(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
            : base(width, height, pixelSizeInBytes, format)
        {
            PixelData = pixelData;
        }
    }
}
