using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLRenderPassStencilAttachmentDescriptor
    {
        public IntPtr NativePtr;

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

        public uint clearStencil
        {
            get => uint_objc_msgSend(NativePtr, "clearStencil");
            set => objc_msgSend(NativePtr, "setClearStencil:", value);
        }

        public UIntPtr slice
        {
            get => UIntPtr_objc_msgSend(NativePtr, "slice");
            set => objc_msgSend(NativePtr, "setSlice:", value);
        }
    }
}