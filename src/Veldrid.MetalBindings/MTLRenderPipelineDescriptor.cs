using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLRenderPipelineDescriptor
    {
        public readonly IntPtr NativePtr;
        public MTLRenderPipelineDescriptor(IntPtr ptr) => NativePtr = ptr;

        public static MTLRenderPipelineDescriptor New()
        {
            var cls = new ObjCClass("MTLRenderPipelineDescriptor");
            var ret = cls.AllocInit<MTLRenderPipelineDescriptor>();
            return ret;
        }

        public MTLFunction vertexFunction
        {
            get => objc_msgSend<MTLFunction>(NativePtr, "vertexFunction");
            set => objc_msgSend(NativePtr, "setVertexFunction:", value.NativePtr);
        }

        public MTLFunction fragmentFunction
        {
            get => objc_msgSend<MTLFunction>(NativePtr, "fragmentFunction");
            set => objc_msgSend(NativePtr, "setFragmentFunction:", value.NativePtr);
        }

        public MTLRenderPipelineColorAttachmentDescriptorArray colorAttachments
            => objc_msgSend<MTLRenderPipelineColorAttachmentDescriptorArray>(NativePtr, "colorAttachments");

        public MTLPixelFormat depthAttachmentPixelFormat
        {
            get => (MTLPixelFormat)uint_objc_msgSend(NativePtr, "depthAttachmentPixelFormat");
            set => objc_msgSend(NativePtr, "setDepthAttachmentPixelFormat:", (uint)value);
        }

        public MTLVertexDescriptor vertexDescriptor => objc_msgSend<MTLVertexDescriptor>(NativePtr, "vertexDescriptor");
    }
}