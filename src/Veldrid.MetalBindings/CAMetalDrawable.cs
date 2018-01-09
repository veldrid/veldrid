using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CAMetalDrawable
    {
        public readonly IntPtr NativePtr;

        public MTLTexture texture => objc_msgSend<MTLTexture>(NativePtr, "texture");

        public bool IsNull => NativePtr == IntPtr.Zero;
    }
}