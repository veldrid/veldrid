using System;
using System.Text;

namespace Veldrid.MetalBindings
{
    public unsafe struct Selector
    {
        public readonly IntPtr NativePtr;

        public Selector(IntPtr ptr)
        {
            NativePtr = ptr;
        }

        public Selector(string name)
        {
            int byteCount = Encoding.UTF8.GetMaxByteCount(name.Length);
            byte* utf8BytesPtr = stackalloc byte[byteCount];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, utf8BytesPtr, byteCount);
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

        public static implicit operator Selector(string s) => new Selector(s);
    }
}