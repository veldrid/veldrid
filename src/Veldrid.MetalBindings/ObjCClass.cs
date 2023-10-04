using System;
using System.Runtime.CompilerServices;

namespace Veldrid.MetalBindings
{
    public unsafe struct ObjCClass
    {
        public readonly IntPtr NativePtr;
        public static implicit operator IntPtr(ObjCClass c) => c.NativePtr;

        [SkipLocalsInit]
        public ObjCClass(string name)
        {
            int byteCount = MTLUtil.UTF8.GetMaxByteCount(name.Length);
            byte* utf8BytesPtr = stackalloc byte[byteCount + 1];
            fixed (char* namePtr = name)
            {
                int actualByteCount = MTLUtil.UTF8.GetBytes(namePtr, name.Length, utf8BytesPtr, byteCount);
                utf8BytesPtr[actualByteCount] = 0;
            }

            NativePtr = ObjectiveCRuntime.objc_getClass(utf8BytesPtr);
        }

        [SkipLocalsInit]
        public IntPtr GetProperty(string propertyName)
        {
            int byteCount = MTLUtil.UTF8.GetMaxByteCount(propertyName.Length);
            byte* utf8BytesPtr = stackalloc byte[byteCount + 1];
            fixed (char* namePtr = propertyName)
            {
                int actualByteCount = MTLUtil.UTF8.GetBytes(namePtr, propertyName.Length, utf8BytesPtr, byteCount);
                utf8BytesPtr[actualByteCount] = 0;
            }

            return ObjectiveCRuntime.class_getProperty(this, utf8BytesPtr);
        }

        public string Name => MTLUtil.GetUtf8String(ObjectiveCRuntime.class_getName(this));

        public T Alloc<T>() where T : struct
        {
            IntPtr value = ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, Selectors.alloc);
            return Unsafe.AsRef<T>(&value);
        }

        public T AllocInit<T>() where T : struct
        {
            IntPtr value = ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, Selectors.alloc);
            ObjectiveCRuntime.objc_msgSend(value, Selectors.init);
            return Unsafe.AsRef<T>(&value);
        }

        public ObjectiveCMethod* class_copyMethodList(out uint count)
        {
            return ObjectiveCRuntime.class_copyMethodList(this, out count);
        }
    }
}
