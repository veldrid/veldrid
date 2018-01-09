using System;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public struct NSError
    {
        public readonly IntPtr NativePtr;
        public string domain => string_objc_msgSend(NativePtr, "domain");
        public string localizedDescription => string_objc_msgSend(NativePtr, "localizedDescription");
    }
}