using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLDepthStencilDescriptor
    {
        public readonly IntPtr NativePtr;
        public MTLDepthStencilDescriptor(IntPtr ptr) => NativePtr = ptr;

        public MTLCompareFunction depthCompareFunction
        {
            get => (MTLCompareFunction)uint_objc_msgSend(NativePtr, "depthCompareFunction");
            set => objc_msgSend(NativePtr, "setDepthCompareFunction:", (uint)value);
        }

        public Bool8 depthWriteEnabled
        {
            get => bool8_objc_msgSend(NativePtr, "isDepthWriteEnabled");
            set => objc_msgSend(NativePtr, "setDepthWriteEnabled:", value);
        }
    }
}