using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MTLTexture
    {
        public readonly IntPtr NativePtr;
        public bool IsNull => NativePtr == IntPtr.Zero;

        public void replaceRegion(
            MTLRegion region,
            UIntPtr mipmapLevel,
            UIntPtr slice,
            void* pixelBytes,
            UIntPtr bytesPerRow,
            UIntPtr bytesPerImage)
        {
            objc_msgSend(NativePtr, "replaceRegion:mipmapLevel:slice:withBytes:bytesPerRow:bytesPerImage:",
                region,
                mipmapLevel,
                slice,
                (IntPtr)pixelBytes,
                bytesPerRow,
                bytesPerImage);
        }
    }
}