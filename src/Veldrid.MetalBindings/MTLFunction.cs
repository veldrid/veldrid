using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;

namespace Veldrid.MetalBindings
{
    public struct MTLFunction
    {
        public readonly IntPtr NativePtr;
        public MTLFunction(IntPtr ptr) => NativePtr = ptr;

        public NSDictionary functionConstantsDictionary => objc_msgSend<NSDictionary>(NativePtr, sel_functionConstantsDictionary);

        private static readonly Selector sel_functionConstantsDictionary = "functionConstantsDictionary";
    }
}