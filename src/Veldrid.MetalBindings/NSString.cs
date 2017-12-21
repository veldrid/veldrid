using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public unsafe struct NSString
    {
        public readonly IntPtr NativePtr;
        public static implicit operator IntPtr(NSString nss) => nss.NativePtr;
        public NSString(IntPtr ptr) => NativePtr = ptr;

        public static NSString New(string s)
        {
            var cls = new ObjCClass("NSString");
            var nss = cls.Alloc<NSString>();

            fixed (char* utf16Ptr = s)
            {
                UIntPtr length = (UIntPtr)s.Length;
                IntPtr newString = IntPtr_objc_msgSend(nss, "initWithCharacters:length:", (IntPtr)utf16Ptr, length);
                return new NSString(newString);
            }
        }

        public string GetValue()
        {
            byte* utf8Ptr = bytePtr_objc_msgSend(NativePtr, "UTF8String");
            return MTLUtil.GetUtf8String(utf8Ptr);
        }
    }
}