using System;

namespace Veldrid.MetalBindings
{
    public struct MTLRenderPipelineState
    {
        public readonly IntPtr NativePtr;
        public MTLRenderPipelineState(IntPtr ptr) => NativePtr = ptr;
    }
}