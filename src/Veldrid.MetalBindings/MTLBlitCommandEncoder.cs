using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLBlitCommandEncoder
    {
        public readonly IntPtr NativePtr;

        public bool IsNull => NativePtr == IntPtr.Zero;

        public void copy(
            MTLBuffer sourceBuffer,
            UIntPtr sourceOffset,
            MTLBuffer destinationBuffer,
            UIntPtr destinationOffset,
            UIntPtr size)
            => objc_msgSend(
                NativePtr,
                "copyFromBuffer:sourceOffset:toBuffer:destinationOffset:size:",
                sourceBuffer, sourceOffset, destinationBuffer, destinationOffset, size);

        public void copyFromBuffer(
            MTLBuffer sourceBuffer,
            UIntPtr sourceOffset,
            UIntPtr sourceBytesPerRow,
            UIntPtr sourceBytesPerImage,
            MTLSize sourceSize,
            MTLTexture destinationTexture,
            UIntPtr destinationSlice,
            UIntPtr destinationLevel,
            MTLOrigin destinationOrigin)
            => objc_msgSend(
                NativePtr,
                "copyFromBuffer:sourceOffset:sourceBytesPerRow:sourceBytesPerImage:sourceSize:toTexture:destinationSlice:destinationLevel:destinationOrigin:",
                sourceBuffer.NativePtr,
                sourceOffset,
                sourceBytesPerRow,
                sourceBytesPerImage,
                sourceSize,
                destinationTexture.NativePtr,
                destinationSlice,
                destinationLevel,
                destinationOrigin);

        public void endEncoding() => objc_msgSend(NativePtr, "endEncoding");
    }
}