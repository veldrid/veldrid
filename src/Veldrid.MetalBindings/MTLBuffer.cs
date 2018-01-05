using System;
using System.Runtime.InteropServices;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MTLBuffer
    {
        public readonly IntPtr NativePtr;
        public MTLBuffer(IntPtr ptr) => NativePtr = ptr;
        public bool IsNull => NativePtr == IntPtr.Zero;

        public void* contents() => ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, "contents").ToPointer();

        public UIntPtr length => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, "length");

        public void didModifyRange(NSRange range)
            => ObjectiveCRuntime.objc_msgSend(NativePtr, "didModifyRange:", range);

        public void addDebugMarker(NSString marker, NSRange range)
            => ObjectiveCRuntime.objc_msgSend(NativePtr, "addDebugMarker:range:", marker.NativePtr, range);

        public void removeAllDebugMarkers()
            => ObjectiveCRuntime.objc_msgSend(NativePtr, "removeAllDebugMarkers");
    }
}