using System;

namespace Veldrid.MetalBindings
{
    public struct MTLFunctionConstantValues
    {
        public readonly IntPtr NativePtr;

        public static MTLFunctionConstantValues New()
        {
            return s_class.AllocInit<MTLFunctionConstantValues>();
        }

        private static readonly ObjCClass s_class = new ObjCClass(nameof(MTLFunctionConstantValues));
    }
}