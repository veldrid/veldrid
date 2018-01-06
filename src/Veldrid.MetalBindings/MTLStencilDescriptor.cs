using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLStencilDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLStencilOperation stencilFailureOperation
        {
            get => (MTLStencilOperation)uint_objc_msgSend(NativePtr, "stencilFailureOperation");
            set => objc_msgSend(NativePtr, "setStencilFailureOperation:", (uint)value);
        }

        public MTLStencilOperation depthFailureOperation
        {
            get => (MTLStencilOperation)uint_objc_msgSend(NativePtr, "depthFailureOperation");
            set => objc_msgSend(NativePtr, "setDepthFailureOperation:", (uint)value);
        }

        public MTLStencilOperation depthStencilPassOperation
        {
            get => (MTLStencilOperation)uint_objc_msgSend(NativePtr, "depthStencilPassOperation");
            set => objc_msgSend(NativePtr, "setDepthStencilPassOperation:", (uint)value);
        }

        public MTLCompareFunction stencilCompareFunction
        {
            get => (MTLCompareFunction)uint_objc_msgSend(NativePtr, "stencilCompareFunction");
            set => objc_msgSend(NativePtr, "setStencilCompareFunction:", (uint)value);
        }

        public uint readMask
        {
            get => uint_objc_msgSend(NativePtr, "readMask");
            set => objc_msgSend(NativePtr, "setReadMask:", value);
        }

        public uint writeMask
        {
            get => uint_objc_msgSend(NativePtr, "writeMask");
            set => objc_msgSend(NativePtr, "setWriteMask:", value);
        }
    }
}