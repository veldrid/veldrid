using ImageSharp;
using ImageSharp.PixelFormats;

namespace Veldrid.RenderDemo
{
    public static class PixelFormatConversion
    {
        public static unsafe void ConvertPixelsUInt16DepthToRgbaFloat(int numPixels, ushort[] depthPixels, Rgba32* colorPixelsPtr)
        {
            for (int i = 0; i < numPixels; i++)
            {
                ushort pixel = depthPixels[i];
                byte normalizedVal = (byte)(((double)pixel / ushort.MaxValue) * byte.MaxValue);
                int colorIndex = i;
                colorPixelsPtr[colorIndex] = new Rgba32(normalizedVal, normalizedVal, normalizedVal, normalizedVal);
            }
        }
    }
}
