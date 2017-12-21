using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLCompileOptions
    {
        public readonly IntPtr NativePtr;
        public static implicit operator IntPtr(MTLCompileOptions mco) => mco.NativePtr;

        public static MTLCompileOptions New()
        {
            var cls = new ObjCClass("MTLCompileOptions");
            return cls.AllocInit<MTLCompileOptions>();
        }

        public Bool8 fastMathEnabled
        {
            get => bool8_objc_msgSend(NativePtr, "fastMathEnabled");
            set => objc_msgSend(NativePtr, "setFastMathEnabled:", value);
        }

        public MTLLanguageVersion languageVersion
        {
            get => (MTLLanguageVersion)uint_objc_msgSend(NativePtr, "languageVersion");
            set => objc_msgSend(NativePtr, "sendLanguageVersion:", (uint)value);
        }
    }
}