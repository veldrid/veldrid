using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct NSArray
    {
        public readonly IntPtr NativePtr;
        public NSArray(IntPtr ptr) => NativePtr = ptr;

        public UIntPtr count => UIntPtr_objc_msgSend(NativePtr, sel_count);
        private static readonly Selector sel_count = "count";
    }
}