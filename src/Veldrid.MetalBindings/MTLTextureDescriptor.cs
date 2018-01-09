using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLTextureDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLTextureType textureType
        {
            get => (MTLTextureType)uint_objc_msgSend(NativePtr, "textureType");
            set => objc_msgSend(NativePtr, "setTextureType:", (uint)value);
        }

        public MTLPixelFormat pixelFormat
        {
            get => (MTLPixelFormat)uint_objc_msgSend(NativePtr, "pixelFormat");
            set => objc_msgSend(NativePtr, "setPixelFormat:", (uint)value);
        }

        public UIntPtr width
        {
            get => UIntPtr_objc_msgSend(NativePtr, "width");
            set => objc_msgSend(NativePtr, "setWidth:", value);
        }

        public UIntPtr height
        {
            get => UIntPtr_objc_msgSend(NativePtr, "height");
            set => objc_msgSend(NativePtr, "setHeight:", value);
        }

        public UIntPtr depth
        {
            get => UIntPtr_objc_msgSend(NativePtr, "depth");
            set => objc_msgSend(NativePtr, "setDepth:", value);
        }

        public UIntPtr mipmapLevelCount
        {
            get => UIntPtr_objc_msgSend(NativePtr, "mipmapLevelCount");
            set => objc_msgSend(NativePtr, "setMipmapLevelCount:", value);
        }

        public UIntPtr sampleCount
        {
            get => UIntPtr_objc_msgSend(NativePtr, "sampleCount");
            set => objc_msgSend(NativePtr, "setSampleCount:", value);
        }

        public UIntPtr arrayLength
        {
            get => UIntPtr_objc_msgSend(NativePtr, "arrayLength");
            set => objc_msgSend(NativePtr, "setArrayLength:", value);
        }

        public MTLResourceOptions resourceOptions
        {
            get => (MTLResourceOptions)uint_objc_msgSend(NativePtr, "resourceOptions");
            set => objc_msgSend(NativePtr, "setResourceOptions:", (uint)value);
        }

        public MTLCPUCacheMode cpuCacheMode
        {
            get => (MTLCPUCacheMode)uint_objc_msgSend(NativePtr, "cpuCacheMode");
            set => objc_msgSend(NativePtr, "setCpuCacheMode:", (uint)value);
        }

        public MTLStorageMode storageMode
        {
            get => (MTLStorageMode)uint_objc_msgSend(NativePtr, "storageMode");
            set => objc_msgSend(NativePtr, "setStorageMode:", (uint)value);
        }

        public MTLTextureUsage textureUsage
        {
            get => (MTLTextureUsage)uint_objc_msgSend(NativePtr, "textureUsage");
            set => objc_msgSend(NativePtr, "setTextureUsage:", (uint)value);
        }
    }
}