using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MTLTexture
    {
        public readonly IntPtr NativePtr;

        public MTLTexture(IntPtr ptr) => NativePtr = ptr;
        public bool IsNull => NativePtr == IntPtr.Zero;

        public void replaceRegion(
            MTLRegion region,
            UIntPtr mipmapLevel,
            UIntPtr slice,
            void* pixelBytes,
            UIntPtr bytesPerRow,
            UIntPtr bytesPerImage)
        {
            objc_msgSend(NativePtr, sel_replaceRegion,
                region,
                mipmapLevel,
                slice,
                (IntPtr)pixelBytes,
                bytesPerRow,
                bytesPerImage);
        }

        public MTLTexture newTextureView(
            MTLPixelFormat pixelFormat,
            MTLTextureType textureType,
            NSRange levelRange,
            NSRange sliceRange)
        {
            IntPtr ret = IntPtr_objc_msgSend(NativePtr, sel_newTextureView,
                (uint)pixelFormat, (uint)textureType, levelRange, sliceRange);
            return new MTLTexture(ret);
        }

        private static readonly Selector sel_replaceRegion = "replaceRegion:mipmapLevel:slice:withBytes:bytesPerRow:bytesPerImage:";
        private static readonly Selector sel_newTextureView = "newTextureViewWithPixelFormat:textureType:levels:slices:";
    }
}