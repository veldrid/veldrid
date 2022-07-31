using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class FixedUtf8String : IDisposable
    {
        private IntPtr _handle;
        private int _numBytes;

        public byte* StringPtr => (byte*)_handle;

        public FixedUtf8String(ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
            {
                return;
            }

            int byteCount = Util.UTF8.GetByteCount(span);
            _handle = Marshal.AllocHGlobal(byteCount + 1);
            _numBytes = byteCount + 1; // Includes null terminator
            int encodedCount = Util.UTF8.GetBytes(span, new Span<byte>(StringPtr, _numBytes));
            Debug.Assert(encodedCount == byteCount);
            StringPtr[encodedCount] = 0;
        }

        public override string ToString() => Util.UTF8.GetString(StringPtr, _numBytes - 1); // Exclude null terminator

        public static implicit operator byte*(FixedUtf8String utf8String) => utf8String.StringPtr;
        public static implicit operator sbyte*(FixedUtf8String utf8String) => (sbyte*)utf8String.StringPtr;
        public static implicit operator IntPtr(FixedUtf8String utf8String) => new(utf8String.StringPtr);
        public static implicit operator FixedUtf8String(string s) => new(s);
        public static implicit operator string(FixedUtf8String utf8String) => utf8String.ToString();

        private void Dispose(bool disposing)
        {
            if (_handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_handle);
                _handle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~FixedUtf8String()
        {
            Dispose(disposing: false);
        }
    }
}
