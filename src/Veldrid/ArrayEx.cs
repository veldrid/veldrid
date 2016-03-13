using System;
using System.Runtime.InteropServices;

namespace Veldrid
{
    public static class ArrayEx
    {
        public static PinnedArray<T> Pin<T>(this T[] array)
        {
            return new PinnedArray<T>(array);
        }

        public struct PinnedArray<T> : IDisposable
        {
            private readonly GCHandle _handle;

            public IntPtr Ptr { get; }

            public PinnedArray(T[] array)
            {
                _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                Ptr = _handle.AddrOfPinnedObject();
            }

            public void Dispose()
            {
                _handle.Free();
            }
        }
    }
}
