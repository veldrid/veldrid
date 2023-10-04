using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Veldrid.Sdl2
{
    internal static class Utilities
    {
        public static Encoding UTF8 { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public static unsafe IntPtr GetNullTerminatedUtf8(ReadOnlySpan<char> text, ref Span<byte> utf8Buffer)
        {
            IntPtr heapPtr = IntPtr.Zero;
            int byteCount = UTF8.GetMaxByteCount(text.Length) + 1;
            if (utf8Buffer.Length < byteCount)
            {
                byteCount = UTF8.GetByteCount(text) + 1;
                heapPtr = Marshal.AllocHGlobal(byteCount);
                utf8Buffer = new Span<byte>((void*)heapPtr, byteCount);
            }

            int bytesWritten = UTF8.GetBytes(text, utf8Buffer);
            utf8Buffer[bytesWritten] = 0; // Add null terminator.

            return heapPtr;
        }

        public static void FreeUtf8(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
