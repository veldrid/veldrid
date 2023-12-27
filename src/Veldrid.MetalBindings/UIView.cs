using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct UIView
    {
        public readonly IntPtr NativePtr;
        public UIView(IntPtr ptr) => NativePtr = ptr;
        
        public CALayer layer => objc_msgSend<CALayer>(NativePtr, sel_layer);

        public CGRect frame => RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? CGRect_objc_msgSend(NativePtr, sel_frame)
            : objc_msgSend_stret<CGRect>(NativePtr, sel_frame);
        
        private static readonly Selector sel_layer = "layer";
        private static readonly Selector sel_frame = "frame";
    }
}
