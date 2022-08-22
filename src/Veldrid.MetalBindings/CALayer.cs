using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct CALayer
    {
        public readonly IntPtr NativePtr;
        public static implicit operator IntPtr(CALayer c) => c.NativePtr;

        public CALayer(IntPtr ptr) => NativePtr = ptr;

        public void addSublayer(IntPtr layer)
        {
            objc_msgSend(NativePtr, "addSublayer:", layer);
        }
    }
}