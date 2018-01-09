using System;

namespace Veldrid.MetalBindings
{
    public struct ObjectiveCMethod
    {
        public readonly IntPtr NativePtr;
        public ObjectiveCMethod(IntPtr ptr) => NativePtr = ptr;
        public static implicit operator IntPtr(ObjectiveCMethod method) => method.NativePtr;
        public static implicit operator ObjectiveCMethod(IntPtr ptr) => new ObjectiveCMethod(ptr);

        public Selector GetSelector() => ObjectiveCRuntime.method_getName(this);
        public string GetName() => GetSelector().Name;
    }
}