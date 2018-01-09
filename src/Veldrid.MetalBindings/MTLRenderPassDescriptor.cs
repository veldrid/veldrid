using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLRenderPassDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLRenderPassColorAttachmentDescriptorArray colorAttachments
            => objc_msgSend<MTLRenderPassColorAttachmentDescriptorArray>(NativePtr, "colorAttachments");

        public MTLRenderPassDepthAttachmentDescriptor depthAttachment
            => objc_msgSend<MTLRenderPassDepthAttachmentDescriptor>(NativePtr, "depthAttachment");

        public MTLRenderPassStencilAttachmentDescriptor stencilAttachment
            => objc_msgSend<MTLRenderPassStencilAttachmentDescriptor>(NativePtr, "stencilAttachment");
    }
}