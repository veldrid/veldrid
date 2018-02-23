using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLTextureDescriptor
    {
        private static readonly ObjCClass s_class = new ObjCClass(nameof(MTLTextureDescriptor));
        public readonly IntPtr NativePtr;
        public static MTLTextureDescriptor New() => s_class.AllocInit<MTLTextureDescriptor>();

        public MTLTextureType textureType
        {
            get => (MTLTextureType)uint_objc_msgSend(NativePtr, sel_textureType);
            set => objc_msgSend(NativePtr, sel_setTextureType, (uint)value);
        }

        public MTLPixelFormat pixelFormat
        {
            get => (MTLPixelFormat)uint_objc_msgSend(NativePtr, Selectors.pixelFormat);
            set => objc_msgSend(NativePtr, Selectors.setPixelFormat, (uint)value);
        }

        public UIntPtr width
        {
            get => UIntPtr_objc_msgSend(NativePtr, sel_width);
            set => objc_msgSend(NativePtr, sel_setWidth, value);
        }

        public UIntPtr height
        {
            get => UIntPtr_objc_msgSend(NativePtr, sel_height);
            set => objc_msgSend(NativePtr, sel_setHeight, value);
        }

        public UIntPtr depth
        {
            get => UIntPtr_objc_msgSend(NativePtr, sel_depth);
            set => objc_msgSend(NativePtr, sel_setDepth, value);
        }

        public UIntPtr mipmapLevelCount
        {
            get => UIntPtr_objc_msgSend(NativePtr, sel_mipmapLevelCount);
            set => objc_msgSend(NativePtr, sel_setMipmapLevelCount, value);
        }

        public UIntPtr sampleCount
        {
            get => UIntPtr_objc_msgSend(NativePtr, sel_sampleCount);
            set => objc_msgSend(NativePtr, sel_setSampleCount, value);
        }

        public UIntPtr arrayLength
        {
            get => UIntPtr_objc_msgSend(NativePtr, sel_arrayLength);
            set => objc_msgSend(NativePtr, sel_setArrayLength, value);
        }

        public MTLResourceOptions resourceOptions
        {
            get => (MTLResourceOptions)uint_objc_msgSend(NativePtr, sel_resourceOptions);
            set => objc_msgSend(NativePtr, sel_setResourceOptions, (uint)value);
        }

        public MTLCPUCacheMode cpuCacheMode
        {
            get => (MTLCPUCacheMode)uint_objc_msgSend(NativePtr, sel_cpuCacheMode);
            set => objc_msgSend(NativePtr, sel_setCpuCacheMode, (uint)value);
        }

        public MTLStorageMode storageMode
        {
            get => (MTLStorageMode)uint_objc_msgSend(NativePtr, sel_storageMode);
            set => objc_msgSend(NativePtr, sel_setStorageMode, (uint)value);
        }

        public MTLTextureUsage textureUsage
        {
            get => (MTLTextureUsage)uint_objc_msgSend(NativePtr, sel_textureUsage);
            set => objc_msgSend(NativePtr, sel_setTextureUsage, (uint)value);
        }

        private static readonly Selector sel_textureType = "textureType";
        private static readonly Selector sel_setTextureType = "setTextureType:";
        private static readonly Selector sel_width = "width";
        private static readonly Selector sel_setWidth = "setWidth:";
        private static readonly Selector sel_height = "height";
        private static readonly Selector sel_setHeight = "setHeight:";
        private static readonly Selector sel_depth = "depth";
        private static readonly Selector sel_setDepth = "setDepth:";
        private static readonly Selector sel_mipmapLevelCount = "mipmapLevelCount";
        private static readonly Selector sel_setMipmapLevelCount = "setMipmapLevelCount:";
        private static readonly Selector sel_sampleCount = "sampleCount";
        private static readonly Selector sel_setSampleCount = "setSampleCount:";
        private static readonly Selector sel_arrayLength = "arrayLength";
        private static readonly Selector sel_setArrayLength = "setArrayLength:";
        private static readonly Selector sel_resourceOptions = "resourceOptions";
        private static readonly Selector sel_setResourceOptions = "setResourceOptions:";
        private static readonly Selector sel_cpuCacheMode = "cpuCacheMode";
        private static readonly Selector sel_setCpuCacheMode = "setCpuCacheMode:";
        private static readonly Selector sel_storageMode = "storageMode";
        private static readonly Selector sel_setStorageMode = "setStorageMode:";
        private static readonly Selector sel_textureUsage = "textureUsage";
        private static readonly Selector sel_setTextureUsage = "setTextureUsage:";
    }
}