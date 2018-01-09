using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public unsafe struct MTLVertexDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLVertexBufferLayoutDescriptorArray layouts
            => objc_msgSend<MTLVertexBufferLayoutDescriptorArray>(NativePtr, "layouts");

        public MTLVertexAttributeDescriptorArray attributes
            => objc_msgSend<MTLVertexAttributeDescriptorArray>(NativePtr, "attributes");
    }
}