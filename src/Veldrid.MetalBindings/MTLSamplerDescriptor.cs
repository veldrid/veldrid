using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLSamplerDescriptor
    {
        private static readonly ObjCClass s_class = new ObjCClass(nameof(MTLSamplerDescriptor));
        public readonly IntPtr NativePtr;
        public static MTLSamplerDescriptor New() => s_class.AllocInit<MTLSamplerDescriptor>();

        public MTLSamplerAddressMode rAddressMode
        {
            get => (MTLSamplerAddressMode)uint_objc_msgSend(NativePtr, sel_rAddressMode);
            set => objc_msgSend(NativePtr, sel_setRAddressMode, (uint)value);
        }

        public MTLSamplerAddressMode sAddressMode
        {
            get => (MTLSamplerAddressMode)uint_objc_msgSend(NativePtr, sel_sAddressMode);
            set => objc_msgSend(NativePtr, sel_setSAddressMode, (uint)value);
        }

        public MTLSamplerAddressMode tAddressMode
        {
            get => (MTLSamplerAddressMode)uint_objc_msgSend(NativePtr, sel_tAddressMode);
            set => objc_msgSend(NativePtr, sel_setTAddressMode, (uint)value);
        }

        public MTLSamplerMinMagFilter minFilter
        {
            get => (MTLSamplerMinMagFilter)uint_objc_msgSend(NativePtr, sel_minFilter);
            set => objc_msgSend(NativePtr, sel_setMinFilter, (uint)value);
        }

        public MTLSamplerMinMagFilter magFilter
        {
            get => (MTLSamplerMinMagFilter)uint_objc_msgSend(NativePtr, sel_magFilter);
            set => objc_msgSend(NativePtr, sel_setMagFilter, (uint)value);
        }

        public MTLSamplerMipFilter mipFilter
        {
            get => (MTLSamplerMipFilter)uint_objc_msgSend(NativePtr, sel_mipFilter);
            set => objc_msgSend(NativePtr, sel_setMipFilter, (uint)value);
        }

        public float lodMinClamp
        {
            get => float_objc_msgSend(NativePtr, sel_lodMinClamp);
            set => objc_msgSend(NativePtr, sel_setLodMinClamp, value);
        }

        public float lodMaxClamp
        {
            get => float_objc_msgSend(NativePtr, sel_lodMaxClamp);
            set => objc_msgSend(NativePtr, sel_setLodMaxClamp, value);
        }

        public Bool8 lodAverage
        {
            get => bool8_objc_msgSend(NativePtr, sel_lodAverage);
            set => objc_msgSend(NativePtr, sel_setLodAverage, value);
        }

        public UIntPtr maxAnisotropy
        {
            get => UIntPtr_objc_msgSend(NativePtr, sel_maxAnisotropy);
            set => objc_msgSend(NativePtr, sel_setMaAnisotropy, value);
        }

        public MTLCompareFunction compareFunction
        {
            get => (MTLCompareFunction)uint_objc_msgSend(NativePtr, sel_compareFunction);
            set => objc_msgSend(NativePtr, sel_setCompareFunction, (uint)value);
        }

        public MTLSamplerBorderColor borderColor
        {
            get => (MTLSamplerBorderColor)uint_objc_msgSend(NativePtr, sel_borderColor);
            set => objc_msgSend(NativePtr, sel_setBorderColor, (uint)value);
        }

        private static readonly Selector sel_rAddressMode = "rAddressMode";
        private static readonly Selector sel_setRAddressMode = "setRAddressMode:";
        private static readonly Selector sel_sAddressMode = "sAddressMode";
        private static readonly Selector sel_setSAddressMode = "setSAddressMode:";
        private static readonly Selector sel_tAddressMode = "tAddressMode";
        private static readonly Selector sel_setTAddressMode = "setTAddressMode:";
        private static readonly Selector sel_minFilter = "minFilter";
        private static readonly Selector sel_setMinFilter = "setMinFilter:";
        private static readonly Selector sel_magFilter = "magFilter";
        private static readonly Selector sel_setMagFilter = "setMagFilter:";
        private static readonly Selector sel_mipFilter = "mipFilter";
        private static readonly Selector sel_setMipFilter = "setMipFilter:";
        private static readonly Selector sel_lodMinClamp = "lodMinClamp";
        private static readonly Selector sel_setLodMinClamp = "setLodMinClamp:";
        private static readonly Selector sel_lodMaxClamp = "lodMaxClamp";
        private static readonly Selector sel_setLodMaxClamp = "setLodMaxClamp:";
        private static readonly Selector sel_lodAverage = "lodAverage";
        private static readonly Selector sel_setLodAverage = "setLodAverage:";
        private static readonly Selector sel_maxAnisotropy = "maxAnisotropy";
        private static readonly Selector sel_setMaAnisotropy = "setMaxAnisotropy:";
        private static readonly Selector sel_compareFunction = "compareFunction";
        private static readonly Selector sel_setCompareFunction = "setCompareFunction:";
        private static readonly Selector sel_borderColor = "borderColor";
        private static readonly Selector sel_setBorderColor = "setBorderColor:";
    }
}