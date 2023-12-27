using System;

namespace Veldrid.MetalBindings
{
    public struct NSDictionary
    {
        public readonly IntPtr NativePtr;

        public UIntPtr count => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_count);
        
        private static readonly Selector sel_count = "count";
    }
}
