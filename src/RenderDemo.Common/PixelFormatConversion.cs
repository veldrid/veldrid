using ImageProcessorCore;

namespace Veldrid.RenderDemo
{
    public static class PixelFormatConversion
    {
        public static unsafe void ConvertPixelsUInt16DepthToRgbaFloat(int numPixels, ushort[] depthPixels, Color[] colorPixels)
        {
            for (int i = 0; i < numPixels; i++)
            {
                ushort pixel = depthPixels[i];
                byte normalizedVal = (byte)(((double)pixel / ushort.MaxValue) * byte.MaxValue);
                int colorIndex = i;
                colorPixels[colorIndex] = new Color(normalizedVal, normalizedVal, normalizedVal, normalizedVal);
            }
        }
    }
}
