using System;

namespace Veldrid.SampleGallery
{
    public static class Util
    {
        public static void DisposeAll<T>(T[] array) where T : IDisposable
        {
            if (array == null) { return; }

            foreach (T t in array)
            {
                t.Dispose();
            }
        }

        internal static void EnsureArraySize(ref CommandBuffer[] array, int length)
        {
            if (array.Length != length)
            {
                Array.Resize(ref array, length);
            }
        }
    }
}
