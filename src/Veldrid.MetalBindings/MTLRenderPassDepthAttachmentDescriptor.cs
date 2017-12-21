using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLRenderPassDepthAttachmentDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLRenderPassDepthAttachmentDescriptor(IntPtr ptr) => NativePtr = ptr;

        public MTLTexture texture
        {
            get => objc_msgSend<MTLTexture>(NativePtr, "texture");
            set => objc_msgSend(NativePtr, "setTexture:", value.NativePtr);
        }

        public MTLLoadAction loadAction
        {
            get => (MTLLoadAction)uint_objc_msgSend(NativePtr, "loadAction");
            set => objc_msgSend(NativePtr, "setLoadAction:", (uint)value);
        }

        public double clearDepth
        {
            get => double_objc_msgSend(NativePtr, "clearDepth");
            set => objc_msgSend(NativePtr, "setClearDepth:", value);
        }
    }
}