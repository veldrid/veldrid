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

        public NSView contentView => objc_msgSend<NSView>(NativePtr, sel_contentView);
        
        private static readonly Selector sel_contentView = "contentView";
    }
}
