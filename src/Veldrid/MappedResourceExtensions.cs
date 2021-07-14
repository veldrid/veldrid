using System;
using System.Runtime.InteropServices;

namespace Veldrid
{
    public static class MappedResourceExtensions
    {
        public static Span<T> AsSpan<T>(this MappedResourceView<T> resource)
            where T : unmanaged
        {
            return MemoryMarshal.CreateSpan(ref resource[0], resource.Count);
        }

        public static Span<T> AsSpan<T>(this MappedResourceView<T> resource, uint start)
            where T : unmanaged
        {
            if (start >= (uint)resource.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            return MemoryMarshal.CreateSpan(ref resource[start], resource.Count - (int)start);
        }

        public static Span<T> AsSpan<T>(this MappedResourceView<T> resource, int start)
            where T : unmanaged
        {
            return resource.AsSpan((uint)start);
        }
    }
}
