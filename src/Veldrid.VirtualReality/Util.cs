using System.Text;

namespace Veldrid.VirtualReality
{
    internal static class Util
    {
        internal static unsafe string GetUtf8String(byte* ptr)
        {
            int count = 0;
            while (ptr[count] != 0)
            {
                count += 1;
            }

            return Encoding.UTF8.GetString(ptr, count);
        }

        internal static TextureSampleCount GetSampleCount(int sampleCount)
        {
            switch (sampleCount)
            {
                case 1:
                    return TextureSampleCount.Count1;
                case 2:
                    return TextureSampleCount.Count2;
                case 4:
                    return TextureSampleCount.Count4;
                case 8:
                    return TextureSampleCount.Count8;
                case 16:
                    return TextureSampleCount.Count16;
                case 32:
                    return TextureSampleCount.Count32;
                default:
                    throw new VeldridException($"Unsupported sample count: {sampleCount}");
            }
        }

        internal static int GetSampleCount(TextureSampleCount sampleCount)
        {
            switch (sampleCount)
            {
                case TextureSampleCount.Count1: return 1;
                case TextureSampleCount.Count2: return 2;
                case TextureSampleCount.Count4: return 4;
                case TextureSampleCount.Count8: return 8;
                case TextureSampleCount.Count16: return 16;
                case TextureSampleCount.Count32: return 32;
                default: throw new VeldridException($"Invalid TextureSampleCount: {sampleCount}.");
            }
        }
    }
}
