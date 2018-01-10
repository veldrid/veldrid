using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public unsafe struct NSView
    {
        public readonly IntPtr NativePtr;
        public NSView(IntPtr ptr) => NativePtr = ptr;

        public Bool8 wantsLayer
        {
            get => bool8_objc_msgSend(NativePtr, "wantsLayer");
            set => objc_msgSend(NativePtr, "setWantsLayer:", value);
        }

        public IntPtr layer
        {
            get => IntPtr_objc_msgSend(NativePtr, "layer");
            set => objc_msgSend(NativePtr, "setLayer:", value);
        }

        public CGRect frame
        {
            get => objc_msgSend_stret<CGRect>(NativePtr, "frame");
        }
    }
}