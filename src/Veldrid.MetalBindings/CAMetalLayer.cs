using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;

namespace Veldrid.MetalBindings
{
    public struct CAMetalLayer
    {
        public readonly IntPtr NativePtr;

        public CAMetalLayer(IntPtr ptr) => NativePtr = ptr;

        public static CAMetalLayer New() => s_class.AllocInit<CAMetalLayer>();

        public static bool TryCast(IntPtr layerPointer, out CAMetalLayer metalLayer)
        {
            var layerObject = new NSObject(layerPointer);

            if (layerObject.IsKindOfClass(s_class))
            {
                metalLayer = new CAMetalLayer(layerPointer);
                return true;
            }

            metalLayer = default;
            return false;
        }

        public MTLDevice device
        {
            get => objc_msgSend<MTLDevice>(NativePtr, sel_device);
            set => objc_msgSend(NativePtr, sel_setDevice, value);
        }

        public MTLPixelFormat pixelFormat
        {
            get => (MTLPixelFormat)uint_objc_msgSend(NativePtr, sel_pixelFormat);
            set => objc_msgSend(NativePtr, sel_setPixelFormat, (uint)value);
        }

        public Bool8 framebufferOnly
        {
            get => bool8_objc_msgSend(NativePtr, sel_framebufferOnly);
            set => objc_msgSend(NativePtr, sel_setFramebufferOnly, value);
        }

        public CGSize drawableSize
        {
            get => CGSize_objc_msgSend(NativePtr, sel_drawableSize);
            set => objc_msgSend(NativePtr, sel_setDrawableSize, value);
        }

        public CGRect frame
        {
            get => CGRect_objc_msgSend(NativePtr, "frame");
            set => objc_msgSend(NativePtr, "setFrame:", value);
        }

        public Bool8 opaque
        {
            get => bool8_objc_msgSend(NativePtr, "isOpaque");
            set => objc_msgSend(NativePtr, "setOpaque:", value);
        }

        public CAMetalDrawable nextDrawable() => objc_msgSend<CAMetalDrawable>(NativePtr, sel_nextDrawable);

        public Bool8 displaySyncEnabled
        {
            get => bool8_objc_msgSend(NativePtr, "displaySyncEnabled");
            set => objc_msgSend(NativePtr, "setDisplaySyncEnabled:", value);
        }

        private static readonly ObjCClass s_class = new ObjCClass(nameof(CAMetalLayer));
        private static readonly Selector sel_device = "device";
        private static readonly Selector sel_setDevice = "setDevice:";
        private static readonly Selector sel_pixelFormat = "pixelFormat";
        private static readonly Selector sel_setPixelFormat = "setPixelFormat:";
        private static readonly Selector sel_framebufferOnly = "framebufferOnly";
        private static readonly Selector sel_setFramebufferOnly = "setFramebufferOnly:";
        private static readonly Selector sel_drawableSize = "drawableSize";
        private static readonly Selector sel_setDrawableSize = "setDrawableSize:";
        private static readonly Selector sel_nextDrawable = "nextDrawable";
    }
}
