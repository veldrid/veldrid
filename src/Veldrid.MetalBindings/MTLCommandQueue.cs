using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLCommandQueue
    {
        public readonly IntPtr NativePtr;

        public MTLCommandBuffer commandBuffer() => objc_msgSend<MTLCommandBuffer>(NativePtr, "commandBuffer");

        public void insertDebugCaptureBoundary() => objc_msgSend(NativePtr, "insertDebugCaptureBoundary");
    }
}