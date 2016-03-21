using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public static class PixelFormatConversion
    {
        public static unsafe void ConvertPixelsUInt16DepthToRgbaFloat(int numPixels, ushort[] depthPixels, RgbaFloat* colorPixels)
        {
            for (int i = 0; i < numPixels; i++)
            {
                ushort pixel = depthPixels[i];
                float normalizedVal = ((float)pixel / ushort.MaxValue);
                colorPixels[i] = new RgbaFloat(normalizedVal, normalizedVal, normalizedVal, 1f);
            }
        }
    }
}
