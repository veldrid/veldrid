using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLRenderCommandEncoder
    {
        public readonly IntPtr NativePtr;
        public MTLRenderCommandEncoder(IntPtr ptr) => NativePtr = ptr;
        public bool IsNull => NativePtr == IntPtr.Zero;

        public void setRenderPipelineState(MTLRenderPipelineState pipelineState)
            => objc_msgSend(NativePtr, "setRenderPipelineState:", pipelineState.NativePtr);

        public void setVertexBuffer(MTLBuffer buffer, UIntPtr offset, UIntPtr index)
            => objc_msgSend(NativePtr, "setVertexBuffer:offset:atIndex:",
                buffer.NativePtr,
                offset,
                index);

        public void setFragmentBuffer(MTLBuffer buffer, UIntPtr offset, UIntPtr index)
            => objc_msgSend(NativePtr, "setFragmentBuffer:offset:atIndex:",
                buffer.NativePtr,
                offset,
                index);

        public void setVertexTexture(MTLTexture texture, UIntPtr index)
            => objc_msgSend(NativePtr, "setVertexTexture:atIndex:", texture.NativePtr, index);
        public void setFragmentTexture(MTLTexture texture, UIntPtr index)
            => objc_msgSend(NativePtr, "setFragmentTexture:atIndex:", texture.NativePtr, index);

        public void setVertexSamplerState(MTLSamplerState sampler, UIntPtr index)
            => objc_msgSend(NativePtr, "setVertexSamplerState:atIndex:", sampler.NativePtr, index);

        public void setFragmentSamplerState(MTLSamplerState sampler, UIntPtr index)
            => objc_msgSend(NativePtr, "setFragmentSamplerState:atIndex:", sampler.NativePtr, index);

        public void drawPrimitives(
            MTLPrimitiveType primitiveType,
            UIntPtr vertexStart,
            UIntPtr vertexCount,
            UIntPtr instanceCount,
            UIntPtr baseInstance)
            => objc_msgSend(NativePtr, "drawPrimitives:vertexStart:vertexCount:instanceCount:baseInstance:",
                primitiveType, vertexStart, vertexCount, instanceCount, baseInstance);

        public void drawPrimitives(MTLPrimitiveType primitiveType, MTLBuffer indirectBuffer, UIntPtr indirectBufferOffset)
            => objc_msgSend(NativePtr, "drawPrimitives:indirectBuffer:indirectBufferOffset:",
                primitiveType, indirectBuffer, indirectBufferOffset);

        public void drawIndexedPrimitives(
            MTLPrimitiveType primitiveType,
            UIntPtr indexCount,
            MTLIndexType indexType,
            MTLBuffer indexBuffer,
            UIntPtr indexBufferOffset)
            => objc_msgSend(NativePtr, "drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:",
                primitiveType, indexCount, indexType, indexBuffer.NativePtr, indexBufferOffset);

        public void drawIndexedPrimitives(
            MTLPrimitiveType primitiveType,
            UIntPtr indexCount,
            MTLIndexType indexType,
            MTLBuffer indexBuffer,
            UIntPtr indexBufferOffset,
            UIntPtr instanceCount,
            IntPtr baseVertex,
            UIntPtr baseInstance)
            => objc_msgSend(
                NativePtr,
                "drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:instanceCount:baseVertex:baseInstance:",
                primitiveType, indexCount, indexType, indexBuffer.NativePtr, indexBufferOffset, instanceCount, baseVertex, baseInstance);

        public void drawIndexedPrimitives(
            MTLPrimitiveType primitiveType,
            MTLIndexType indexType,
            MTLBuffer indexBuffer,
            UIntPtr indexBufferOffset,
            MTLBuffer indirectBuffer,
            UIntPtr indirectBufferOffset)
            => objc_msgSend(NativePtr, "drawIndexedPrimitives:indexType:indexBuffer:indexBufferOffset:indirectBuffer:indirectBufferOffset:",
                primitiveType,
                indexType,
                indexBuffer,
                indexBufferOffset,
                indirectBuffer,
                indirectBufferOffset);

        public unsafe void setViewports(MTLViewport* viewports, UIntPtr count)
            => objc_msgSend(NativePtr, "setViewports:count:", viewports, count);

        public unsafe void setScissorRects(MTLScissorRect* scissorRects, UIntPtr count)
            => objc_msgSend(NativePtr, "setScissorRects:count:", scissorRects, count);

        public void setCullMode(MTLCullMode cullMode)
            => objc_msgSend(NativePtr, "setCullMode:", (uint)cullMode);

        public void setFrontFacing(MTLWinding frontFaceWinding)
            => objc_msgSend(NativePtr, "setFrontFacingWinding:", (uint)frontFaceWinding);

        public void setDepthStencilState(MTLDepthStencilState depthStencilState)
            => objc_msgSend(NativePtr, "setDepthStencilState:", depthStencilState.NativePtr);

        public void setDepthClipMode(MTLDepthClipMode depthClipMode)
            => objc_msgSend(NativePtr, "setDepthClipMode:", (uint)depthClipMode);

        public void endEncoding() => objc_msgSend(NativePtr, "endEncoding");
    }
}