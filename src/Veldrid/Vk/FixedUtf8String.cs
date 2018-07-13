using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Veldrid.Vk
{
    internal unsafe class FixedUtf8String : IDisposable
    {
        private GCHandle _handle;
        private uint _numBytes;

        public byte* StringPtr => (byte*)_handle.AddrOfPinnedObject().ToPointer();

        public FixedUtf8String(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            int byteCount = Encoding.UTF8.GetByteCount(s);
            byte[] text = new byte[byteCount + 1];
            _handle = GCHandle.Alloc(text, GCHandleType.Pinned);
            _numBytes = (uint)text.Length - 1; // Includes null terminator
            int encodedCount = Encoding.UTF8.GetBytes(s, 0, s.Length, text, 0);
            Debug.Assert(encodedCount == byteCount);
        }

        private string GetString()
        {
            return Encoding.UTF8.GetString(StringPtr, (int)_numBytes);
        }

        public void Dispose()
        {
            _handle.Free();
        }

        public override string ToString() => GetString();

        public static implicit operator byte* (FixedUtf8String utf8String) => utf8String.StringPtr;
        public static implicit operator IntPtr(FixedUtf8String utf8String) => new IntPtr(utf8String.StringPtr);
        public static implicit operator FixedUtf8String(string s) => new FixedUtf8String(s);
        public static implicit operator string(FixedUtf8String utf8String) => utf8String.GetString();
    }
}
