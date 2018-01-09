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
            get => (MTLPixelFormat)uint_objc_msgSend(NativePtr, "pixelFormat");
            set => objc_msgSend(NativePtr, "setPixelFormat:", (uint)value);
        }

        public Bool8 blendingEnabled
        {
            get => bool8_objc_msgSend(NativePtr, "isBlendingEnabled");
            set => objc_msgSend(NativePtr, "setBlendingEnabled:", value);
        }

        public MTLBlendOperation alphaBlendOperation
        {
            get => (MTLBlendOperation)uint_objc_msgSend(NativePtr, "alphaBlendOperation");
            set => objc_msgSend(NativePtr, "setAlphaBlendOperation:", (uint)value);
        }

        public MTLBlendOperation rgbBlendOperation
        {
            get => (MTLBlendOperation)uint_objc_msgSend(NativePtr, "rgbBlendOperation");
            set => objc_msgSend(NativePtr, "setRgbBlendOperation:", (uint)value);
        }

        public MTLBlendFactor destinationAlphaBlendFactor
        {
            get => (MTLBlendFactor)uint_objc_msgSend(NativePtr, "destinationAlphaBlendFactor");
            set => objc_msgSend(NativePtr, "setDestinationAlphaBlendFactor:", (uint)value);
        }

        public MTLBlendFactor destinationRGBBlendFactor
        {
            get => (MTLBlendFactor)uint_objc_msgSend(NativePtr, "destinationRGBBlendFactor");
            set => objc_msgSend(NativePtr, "setDestinationRGBBlendFactor:", (uint)value);
        }

        public MTLBlendFactor sourceAlphaBlendFactor
        {
            get => (MTLBlendFactor)uint_objc_msgSend(NativePtr, "sourceAlphaBlendFactor");
            set => objc_msgSend(NativePtr, "setSourceAlphaBlendFactor:", (uint)value);
        }

        public MTLBlendFactor sourceRGBBlendFactor
        {
            get => (MTLBlendFactor)uint_objc_msgSend(NativePtr, "sourceRGBBlendFactor");
            set => objc_msgSend(NativePtr, "setSourceRGBBlendFactor:", (uint)value);
        }

    }
}