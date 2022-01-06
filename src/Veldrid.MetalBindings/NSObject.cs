using static Veldrid.MetalBindings.ObjectiveCRuntime;
using System;

namespace Veldrid.MetalBindings
{
    public struct NSObject
    {
        public readonly IntPtr NativePtr;

        public NSObject(IntPtr ptr) => NativePtr = ptr;

        public Bool8 IsKindOfClass(IntPtr @class) => bool8_objc_msgSend(NativePtr, sel_isKindOfClass, @class);

        private static readonly Selector sel_isKindOfClass = "isKindOfClass:";
    }
}
