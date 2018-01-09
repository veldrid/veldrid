using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLVertexAttributeDescriptor
    {
        public readonly IntPtr NativePtr;
        public MTLVertexAttributeDescriptor(IntPtr ptr) => NativePtr = ptr;
        
        public MTLVertexFormat format
        {
            get => (MTLVertexFormat)uint_objc_msgSend(NativePtr, "format");
            set => objc_msgSend(NativePtr, "setFormat:", (uint)value);
        }

        public UIntPtr offset
        {
            get => UIntPtr_objc_msgSend(NativePtr, "offset");
            set => objc_msgSend(NativePtr, "setOffset:", value);
        }

        public UIntPtr bufferIndex
        {
            get => UIntPtr_objc_msgSend(NativePtr, "bufferIndex");
            set => objc_msgSend(NativePtr, "setBufferIndex:", value);
        }
    }
}