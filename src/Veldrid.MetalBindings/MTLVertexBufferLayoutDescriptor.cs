using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLVertexBufferLayoutDescriptor
    {
        public readonly IntPtr NativePtr;
        public MTLVertexBufferLayoutDescriptor(IntPtr ptr) => NativePtr = ptr;

        public MTLVertexStepFunction stepFunction
        {
            get => (MTLVertexStepFunction)uint_objc_msgSend(NativePtr, "stepFunction");
            set => objc_msgSend(NativePtr, "setStepFunction:", (uint)value);
        }

        public UIntPtr stride
        {
            get => UIntPtr_objc_msgSend(NativePtr, "stride");
            set => objc_msgSend(NativePtr, "setStride:", value);
        }

        public UIntPtr stepRate
        {
            get => UIntPtr_objc_msgSend(NativePtr, "stepRate");
            set => objc_msgSend(NativePtr, "setStepRate:", value);
        }
    }
}