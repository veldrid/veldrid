using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;

namespace Veldrid.MetalBindings
{
    public struct CAMetalLayer
    {
        public readonly IntPtr NativePtr;
        public CAMetalLayer(IntPtr ptr) => NativePtr = ptr;

        public static CAMetalLayer New()
        {
            var cls = new ObjCClass("CAMetalLayer");
            return cls.AllocInit<CAMetalLayer>();
        }

        public MTLDevice device
        {
            get => objc_msgSend<MTLDevice>(NativePtr, "device");
            set => objc_msgSend(NativePtr, "setDevice:", value);
        }

        public MTLPixelFormat pixelFormat
        {
            get => (MTLPixelFormat)uint_objc_msgSend(NativePtr, "pixelFormat");
            set => objc_msgSend(NativePtr, "setPixelFormat:", (uint)value);
        }

        public Bool8 framebufferOnly
        {
            get => bool8_objc_msgSend(NativePtr, "framebufferOnly");
            set => objc_msgSend(NativePtr, "setFramebufferOnly:", value);
        }

        public CGSize drawableSize
        {
            get => CGSize_objc_msgSend(NativePtr, "drawableSize");
            set => objc_msgSend(NativePtr, "setDrawableSize:", value);
        }

        public CAMetalDrawable nextDrawable() => objc_msgSend<CAMetalDrawable>(NativePtr, "nextDrawable");
    }
}