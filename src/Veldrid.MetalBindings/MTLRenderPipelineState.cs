using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLRenderPipelineState
    {
        public readonly IntPtr NativePtr;
        public MTLRenderPipelineState(IntPtr ptr) => NativePtr = ptr;
    }
}