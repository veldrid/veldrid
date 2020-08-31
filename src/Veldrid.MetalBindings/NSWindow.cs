using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public unsafe struct NSWindow
    {
        public readonly IntPtr NativePtr;
        public NSWindow(IntPtr ptr)
        {
            NativePtr = ptr;
        }

        public NSView contentView => objc_msgSend<NSView>(NativePtr, "contentView");

        public CGFloat backingScaleFactor => CGFloat_objc_msgSend(NativePtr, "backingScaleFactor");

        public CGRect convertRectToBacking(CGRect rect) => objc_msgSend_stret<CGRect>(NativePtr, "convertRectToBacking:", rect);
    }
}