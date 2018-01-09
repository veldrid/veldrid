using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLPipelineBufferDescriptor
    {
        public readonly IntPtr NativePtr;
        public MTLPipelineBufferDescriptor(IntPtr ptr) => NativePtr = ptr;

        public MTLMutability mutability
        {
            get => (MTLMutability)uint_objc_msgSend(NativePtr, "mutability");
            set => objc_msgSend(NativePtr, "setMutability:", (uint)value);
        }
    }
}