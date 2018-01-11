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
            get => (MTLVertexFormat)uint_objc_msgSend(NativePtr, sel_format);
            set => objc_msgSend(NativePtr, sel_setFormat, (uint)value);
        }

        public UIntPtr offset
        {
            get => UIntPtr_objc_msgSend(NativePtr, sel_offset);
            set => objc_msgSend(NativePtr, sel_setOffset, value);
        }

        public UIntPtr bufferIndex
        {
            get => UIntPtr_objc_msgSend(NativePtr, sel_bufferIndex);
            set => objc_msgSend(NativePtr, sel_setBufferIndex, value);
        }

        private static readonly Selector sel_format = "format";
        private static readonly Selector sel_setFormat = "setFormat:";
        private static readonly Selector sel_offset = "offset";
        private static readonly Selector sel_setOffset = "setOffset:";
        private static readonly Selector sel_bufferIndex = "bufferIndex";
        private static readonly Selector sel_setBufferIndex = "setBufferIndex:";
    }
}