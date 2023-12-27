using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public unsafe struct NSView
    {
        public readonly IntPtr NativePtr;
        public static implicit operator IntPtr(NSView nsView) => nsView.NativePtr;

        public NSView(IntPtr ptr) => NativePtr = ptr;

        public Bool8 wantsLayer
        {
            get => bool8_objc_msgSend(NativePtr, sel_wantsLayer);
            set => objc_msgSend(NativePtr, sel_setWantsLayer, value);
        }

        public IntPtr layer
        {
            get => IntPtr_objc_msgSend(NativePtr, sel_layer);
            set => objc_msgSend(NativePtr, sel_setLayer, value);
        }

        public CGRect frame => RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? CGRect_objc_msgSend(NativePtr, sel_frame)
            : objc_msgSend_stret<CGRect>(NativePtr, sel_frame);
        
        private static readonly Selector sel_wantsLayer = "wantsLayer";
        private static readonly Selector sel_setWantsLayer = "setWantsLayer:";
        private static readonly Selector sel_layer = "layer";
        private static readonly Selector sel_setLayer = "setLayer:";
        private static readonly Selector sel_frame = "frame";
    }
}
