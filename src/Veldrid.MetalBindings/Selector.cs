using System;
using System.Runtime.CompilerServices;

namespace Veldrid.MetalBindings
{
    public unsafe struct Selector
    {
        public readonly IntPtr NativePtr;

        public Selector(IntPtr ptr)
        {
            NativePtr = ptr;
        }

        [SkipLocalsInit]
        public Selector(string name)
        {
            int byteCount = MTLUtil.UTF8.GetMaxByteCount(name.Length);
            byte* utf8BytesPtr = stackalloc byte[byteCount + 1];
            fixed (char* namePtr = name)
            {
                int actualByteCount = MTLUtil.UTF8.GetBytes(namePtr, name.Length, utf8BytesPtr, byteCount);
                utf8BytesPtr[actualByteCount] = 0;
            }

            NativePtr = ObjectiveCRuntime.sel_registerName(utf8BytesPtr);
        }

        public string Name
        {
            get
            {
                byte* name = ObjectiveCRuntime.sel_getName(NativePtr);
                return MTLUtil.GetUtf8String(name);
            }
        }

        public static implicit operator Selector(string s) => new(s);
    }
}
