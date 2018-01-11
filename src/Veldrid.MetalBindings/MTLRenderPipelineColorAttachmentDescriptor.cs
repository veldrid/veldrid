using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLRenderPipelineColorAttachmentDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLRenderPipelineColorAttachmentDescriptor(IntPtr ptr) => NativePtr = ptr;

        public MTLPixelFormat pixelFormat
        {
            get => (MTLPixelFormat)uint_objc_msgSend(NativePtr, Selectors.pixelFormat);
            set => objc_msgSend(NativePtr, Selectors.setPixelFormat, (uint)value);
        }

        public Bool8 blendingEnabled
        {
            get => bool8_objc_msgSend(NativePtr, sel_isBlendingEnabled);
            set => objc_msgSend(NativePtr, sel_setBlendingEnabled, value);
        }

        public MTLBlendOperation alphaBlendOperation
        {
            get => (MTLBlendOperation)uint_objc_msgSend(NativePtr, sel_alphaBlendOperation);
            set => objc_msgSend(NativePtr, sel_setAlphaBlendOperation, (uint)value);
        }

        public MTLBlendOperation rgbBlendOperation
        {
            get => (MTLBlendOperation)uint_objc_msgSend(NativePtr, sel_rgbBlendOperation);
            set => objc_msgSend(NativePtr, sel_setRGBBlendOperation, (uint)value);
        }

        public MTLBlendFactor destinationAlphaBlendFactor
        {
            get => (MTLBlendFactor)uint_objc_msgSend(NativePtr, sel_destinationAlphaBlendFactor);
            set => objc_msgSend(NativePtr, sel_setDestinationAlphaBlendFactor, (uint)value);
        }

        public MTLBlendFactor destinationRGBBlendFactor
        {
            get => (MTLBlendFactor)uint_objc_msgSend(NativePtr, sel_destinationRGBBlendFactor);
            set => objc_msgSend(NativePtr, sel_setDestinationRGBBlendFactor, (uint)value);
        }

        public MTLBlendFactor sourceAlphaBlendFactor
        {
            get => (MTLBlendFactor)uint_objc_msgSend(NativePtr, sel_sourceAlphaBlendFactor);
            set => objc_msgSend(NativePtr, sel_setSourceAlphaBlendFactor, (uint)value);
        }

        public MTLBlendFactor sourceRGBBlendFactor
        {
            get => (MTLBlendFactor)uint_objc_msgSend(NativePtr, sel_sourceRGBBlendFactor);
            set => objc_msgSend(NativePtr, sel_setSourceRGBBlendFactor, (uint)value);
        }

        private static readonly Selector sel_isBlendingEnabled = "isBlendingEnabled";
        private static readonly Selector sel_setBlendingEnabled = "setBlendingEnabled:";
        private static readonly Selector sel_alphaBlendOperation = "alphaBlendOperation";
        private static readonly Selector sel_setAlphaBlendOperation = "setAlphaBlendOperation:";
        private static readonly Selector sel_rgbBlendOperation = "rgbBlendOperation";
        private static readonly Selector sel_setRGBBlendOperation = "setRgbBlendOperation:";
        private static readonly Selector sel_destinationAlphaBlendFactor = "destinationAlphaBlendFactor";
        private static readonly Selector sel_setDestinationAlphaBlendFactor = "setDestinationAlphaBlendFactor:";
        private static readonly Selector sel_destinationRGBBlendFactor = "destinationRGBBlendFactor";
        private static readonly Selector sel_setDestinationRGBBlendFactor = "setDestinationRGBBlendFactor:";
        private static readonly Selector sel_sourceAlphaBlendFactor = "sourceAlphaBlendFactor";
        private static readonly Selector sel_setSourceAlphaBlendFactor = "setSourceAlphaBlendFactor:";
        private static readonly Selector sel_sourceRGBBlendFactor = "sourceRGBBlendFactor";
        private static readonly Selector sel_setSourceRGBBlendFactor = "setSourceRGBBlendFactor:";
    }
}