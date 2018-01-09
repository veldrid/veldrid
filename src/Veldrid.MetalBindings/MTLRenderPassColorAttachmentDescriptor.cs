using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLRenderPassColorAttachmentDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLRenderPassColorAttachmentDescriptor(IntPtr ptr) => NativePtr = ptr;

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

        public MTLStoreAction storeAction
        {
            get => (MTLStoreAction)uint_objc_msgSend(NativePtr, "storeAction");
            set => objc_msgSend(NativePtr, "setStoreAction:", (uint)value);
        }

        public MTLTexture resolveTexture
        {
            get => objc_msgSend<MTLTexture>(NativePtr, "resolveTexture");
            set => objc_msgSend(NativePtr, "setResolveTexture:", value.NativePtr);
        }

        public MTLClearColor clearColor
        {
            get => objc_msgSend_stret<MTLClearColor>(NativePtr, "clearColor");
            set => objc_msgSend(NativePtr, "setClearColor:", value);
        }

        public UIntPtr slice
        {
            get => UIntPtr_objc_msgSend(NativePtr, "slice");
            set => objc_msgSend(NativePtr, "setSlice:", value);
        }
    }
}