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
    }
}