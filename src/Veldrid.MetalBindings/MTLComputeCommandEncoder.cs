using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLComputeCommandEncoder
    {
        public readonly IntPtr NativePtr;
        public bool IsNull => NativePtr == IntPtr.Zero;

        public void setComputePipelineState(MTLComputePipelineState state)
            => objc_msgSend(NativePtr, "setComputePipelineState:", state.NativePtr);

        public void setBuffer(MTLBuffer buffer, UIntPtr offset, UIntPtr index)
            => objc_msgSend(NativePtr, "setBuffer:offset:atIndex:",
                buffer.NativePtr,
                offset,
                index);

        public void dispatchThreadGroups(MTLSize threadgroupsPerGrid, MTLSize threadsPerThreadgroup)
            => objc_msgSend(NativePtr, "dispatchThreadgroups:threadsPerThreadgroup:",
                threadgroupsPerGrid, threadsPerThreadgroup);

        public void dispatchThreadgroupsWithIndirectBuffer(
            MTLBuffer indirectBuffer,
            UIntPtr indirectBufferOffset,
            MTLSize threadsPerThreadgroup)
            => objc_msgSend(NativePtr, "dispatchThreadgroupsWithIndirectBuffer:indirectBufferOffset:threadsPerThreadgroup:",
                indirectBuffer.NativePtr,
                indirectBufferOffset,
                threadsPerThreadgroup);

        public void endEncoding() => objc_msgSend(NativePtr, "endEncoding");

        public void setTexture(MTLTexture texture, UIntPtr index)
            => objc_msgSend(NativePtr, "setTexture:", texture.NativePtr, index);

        public void setSamplerState(MTLSamplerState sampler, UIntPtr index)
            => objc_msgSend(NativePtr, "setSamplerState:", sampler.NativePtr, index);
    }
}