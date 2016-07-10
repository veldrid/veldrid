using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public static class PixelFormatConversion
    {
        public static unsafe void ConvertPixelsUInt16DepthToRgbaFloat(int numPixels, ushort[] depthPixels, float[] colorPixels)
        {
            for (int i = 0; i < numPixels; i++)
            {
                ushort pixel = depthPixels[i];
                float normalizedVal = ((float)pixel / ushort.MaxValue);
                int colorIndex = i * 4;
                colorPixels[colorIndex] = normalizedVal;
                colorPixels[colorIndex + 1] = normalizedVal;
                colorPixels[colorIndex + 2] = normalizedVal;
                colorPixels[colorIndex + 3] = 1;
            }
        }
    }
}
