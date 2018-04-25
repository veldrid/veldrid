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
            get => objc_msgSend<MTLTexture>(NativePtr, Selectors.texture);
            set => objc_msgSend(NativePtr, Selectors.setTexture, value.NativePtr);
        }

        public MTLLoadAction loadAction
        {
            get => (MTLLoadAction)uint_objc_msgSend(NativePtr, Selectors.loadAction);
            set => objc_msgSend(NativePtr, Selectors.setLoadAction, (uint)value);
        }

        public MTLStoreAction storeAction
        {
            get => (MTLStoreAction)uint_objc_msgSend(NativePtr, Selectors.storeAction);
            set => objc_msgSend(NativePtr, Selectors.setStoreAction, (uint)value);
        }

        public double clearDepth
        {
            get => double_objc_msgSend(NativePtr, sel_clearDepth);
            set => objc_msgSend(NativePtr, sel_setClearDepth, value);
        }

        public UIntPtr slice
        {
            get => UIntPtr_objc_msgSend(NativePtr, Selectors.slice);
            set => objc_msgSend(NativePtr, Selectors.setSlice, value);
        }

        public UIntPtr level
        {
            get => UIntPtr_objc_msgSend(NativePtr, Selectors.level);
            set => objc_msgSend(NativePtr, Selectors.setLevel, value);
        }

        private static readonly Selector sel_clearDepth = "clearDepth";
        private static readonly Selector sel_setClearDepth = "setClearDepth:";
    }
}