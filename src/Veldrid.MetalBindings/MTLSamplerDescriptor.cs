using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLSamplerDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLSamplerAddressMode rAddressMode
        {
            get => (MTLSamplerAddressMode)uint_objc_msgSend(NativePtr, "rAddressMode");
            set => objc_msgSend(NativePtr, "setRAddressMode:", (uint)value);
        }

        public MTLSamplerAddressMode sAddressMode
        {
            get => (MTLSamplerAddressMode)uint_objc_msgSend(NativePtr, "sAddressMode");
            set => objc_msgSend(NativePtr, "setSAddressMode:", (uint)value);
        }

        public MTLSamplerAddressMode tAddressMode
        {
            get => (MTLSamplerAddressMode)uint_objc_msgSend(NativePtr, "tAddressMode");
            set => objc_msgSend(NativePtr, "setTAddressMode:", (uint)value);
        }

        public MTLSamplerMinMagFilter minFilter
        {
            get => (MTLSamplerMinMagFilter)uint_objc_msgSend(NativePtr, "minFilter");
            set => objc_msgSend(NativePtr, "setMinFilter:", (uint)value);
        }

        public MTLSamplerMinMagFilter magFilter
        {
            get => (MTLSamplerMinMagFilter)uint_objc_msgSend(NativePtr, "magFilter");
            set => objc_msgSend(NativePtr, "setMagFilter:", (uint)value);
        }

        public MTLSamplerMipFilter mipFilter
        {
            get => (MTLSamplerMipFilter)uint_objc_msgSend(NativePtr, "mipFilter");
            set => objc_msgSend(NativePtr, "setMipFilter:", (uint)value);
        }

        public float lodMinClamp
        {
            get => float_objc_msgSend(NativePtr, "lodMinClamp");
            set => objc_msgSend(NativePtr, "setLodMinClamp:", value);
        }

        public float lodMaxClamp
        {
            get => float_objc_msgSend(NativePtr, "lodMaxClamp");
            set => objc_msgSend(NativePtr, "setLodMaxClamp:", value);
        }

        public Bool8 lodAverage
        {
            get => bool8_objc_msgSend(NativePtr, "lodAverage");
            set => objc_msgSend(NativePtr, "setLodAverage:", value);
        }

        public UIntPtr maxAnisotropy
        {
            get => UIntPtr_objc_msgSend(NativePtr, "maxAnisotropy");
            set => objc_msgSend(NativePtr, "setMaxAnisotropy:", value);
        }

        public MTLCompareFunction compareFunction
        {
            get => (MTLCompareFunction)uint_objc_msgSend(NativePtr, "compareFunction");
            set => objc_msgSend(NativePtr, "setCompareFunction:", (uint)value);
        }

        public MTLSamplerBorderColor borderColor
        {
            get => (MTLSamplerBorderColor)uint_objc_msgSend(NativePtr, "borderColor");
            set => objc_msgSend(NativePtr, "setBorderColor:", (uint)value);
        }
    }
}