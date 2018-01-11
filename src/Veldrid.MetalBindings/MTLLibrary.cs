using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLLibrary
    {
        public readonly IntPtr NativePtr;
        public MTLLibrary(IntPtr ptr) => NativePtr = ptr;

        public MTLFunction newFunctionWithName(string name)
        {
            NSString nameNSS = NSString.New(name);
            IntPtr function = IntPtr_objc_msgSend(NativePtr, sel_newFunctionWithName, nameNSS);
            return new MTLFunction(function);
        }

        private static readonly Selector sel_newFunctionWithName = "newFunctionWithName:";
    }
}