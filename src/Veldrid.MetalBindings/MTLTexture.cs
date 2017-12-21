using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLTexture
    {
        public readonly IntPtr NativePtr;
        public bool IsNull => NativePtr == IntPtr.Zero;
    }
}