using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct MTLComputePipelineState
    {
        public readonly IntPtr NativePtr;
        public MTLComputePipelineState(IntPtr ptr) => NativePtr = ptr;
        public bool IsNull => NativePtr == IntPtr.Zero;
        public string label
        {
            get => string_objc_msgSend(NativePtr, Selectors.label);
            set
            {
                NSString valueNSS = NSString.New(value);
                objc_msgSend(NativePtr, Selectors.setLabel, valueNSS.NativePtr);
                release(valueNSS.NativePtr);
            }
        }
    }
}