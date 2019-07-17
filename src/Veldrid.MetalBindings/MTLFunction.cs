using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;

namespace Veldrid.MetalBindings
{
    public struct MTLFunction
    {
        public readonly IntPtr NativePtr;
        public MTLFunction(IntPtr ptr) => NativePtr = ptr;

        public NSDictionary functionConstantsDictionary => objc_msgSend<NSDictionary>(NativePtr, sel_functionConstantsDictionary);

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

        private static readonly Selector sel_functionConstantsDictionary = "functionConstantsDictionary";
    }
}