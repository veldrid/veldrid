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
            return sampleCount switch
            {
                1 => TextureSampleCount.Count1,
                2 => TextureSampleCount.Count2,
                4 => TextureSampleCount.Count4,
                8 => TextureSampleCount.Count8,
                16 => TextureSampleCount.Count16,
                32 => TextureSampleCount.Count32,
                _ => throw new VeldridException($"Unsupported sample count: {sampleCount}"),
            };
        }

        internal static int GetSampleCount(TextureSampleCount sampleCount)
        {
            return sampleCount switch
            {
                TextureSampleCount.Count1 => 1,
                TextureSampleCount.Count2 => 2,
                TextureSampleCount.Count4 => 4,
                TextureSampleCount.Count8 => 8,
                TextureSampleCount.Count16 => 16,
                TextureSampleCount.Count32 => 32,
                _ => throw new VeldridException($"Invalid TextureSampleCount: {sampleCount}."),
            };
        }
    }
}
