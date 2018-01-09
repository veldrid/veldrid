using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLCommandBuffer
    {
        public readonly IntPtr NativePtr;

        public MTLRenderCommandEncoder renderCommandEncoderWithDescriptor(MTLRenderPassDescriptor desc)
        {
            return new MTLRenderCommandEncoder(
                IntPtr_objc_msgSend(NativePtr, "renderCommandEncoderWithDescriptor:", desc.NativePtr));
        }

        public void presentDrawable(IntPtr drawable) => objc_msgSend(NativePtr, "presentDrawable:", drawable);

        public void commit() => objc_msgSend(NativePtr, "commit");

        public MTLBlitCommandEncoder blitCommandEncoder()
            => objc_msgSend<MTLBlitCommandEncoder>(NativePtr, "blitCommandEncoder");

        public MTLComputeCommandEncoder computeCommandEncoder()
            => objc_msgSend<MTLComputeCommandEncoder>(NativePtr, "computeCommandEncoder");

        public void waitUntilCompleted() => objc_msgSend(NativePtr, "waitUntilCompleted");

        public void addCompletedHandler(MTLCommandBufferHandler block)
            => objc_msgSend(NativePtr, "addCompletedHandler:", block);
        public void addCompletedHandler(IntPtr block)
            => objc_msgSend(NativePtr, "addCompletedHandler:", block);

        public MTLCommandBufferStatus status => (MTLCommandBufferStatus)uint_objc_msgSend(NativePtr, "status");
    }
}