using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLComputePipelineDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLFunction computeFunction
        {
            get => objc_msgSend<MTLFunction>(NativePtr, "computeFunction");
            set => objc_msgSend(NativePtr, "setComputeFunction:", value.NativePtr);
        }

        public MTLPipelineBufferDescriptorArray buffers
            => objc_msgSend<MTLPipelineBufferDescriptorArray>(NativePtr, "buffers");
    }
}