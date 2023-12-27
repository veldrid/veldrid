using System;
using System.Runtime.InteropServices;
using Veldrid.MetalBindings;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.OpenGL.EAGL
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CAEAGLLayer
    {
        public readonly IntPtr NativePtr;

        public static CAEAGLLayer New()
        {
            return MTLUtil.AllocInit<CAEAGLLayer>("CAEAGLLayer");
        }

        public CGRect frame
        {
            get => CGRect_objc_msgSend(NativePtr, sel_frame);
            set => objc_msgSend(NativePtr, sel_setFrame, value);
        }

        public Bool8 opaque
        {
            get => bool8_objc_msgSend(NativePtr, sel_isOpaque);
            set => objc_msgSend(NativePtr, sel_setOpaque, value);
        }
        
        public void removeFromSuperlayer() => objc_msgSend(NativePtr, sel_removeFromSuperlayer);

        public void Release() => release(NativePtr);
        
        private static readonly Selector sel_frame = "frame";
        private static readonly Selector sel_setFrame = "setFrame:";
        private static readonly Selector sel_isOpaque = "isOpaque";
        private static readonly Selector sel_setOpaque = "setOpaque:";
        private static readonly Selector sel_removeFromSuperlayer = "removeFromSuperlayer";
    }
}
