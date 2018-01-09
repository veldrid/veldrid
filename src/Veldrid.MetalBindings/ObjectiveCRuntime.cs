using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Veldrid.MetalBindings
{
    public static unsafe class ObjectiveCRuntime
    {
        private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, float a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, double a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, IntPtr a, uint b);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, IntPtr a, NSRange b);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, MTLSize a, MTLSize b);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, IntPtr c, UIntPtr d, MTLSize e);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, MTLClearColor a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, CGSize a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, IntPtr a, UIntPtr b, UIntPtr c);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, MTLPrimitiveType a, UIntPtr b, UIntPtr c, UIntPtr d, UIntPtr e);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, NSRange a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, UIntPtr a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, MTLCommandBufferHandler a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, IntPtr a, UIntPtr b);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, MTLViewport a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, void* a, UIntPtr b);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, MTLPrimitiveType a, UIntPtr b, MTLIndexType c, IntPtr d, UIntPtr e);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, MTLPrimitiveType a, MTLBuffer b, UIntPtr c);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(
            IntPtr receiver,
            Selector selector,
            MTLPrimitiveType a,
            UIntPtr b,
            MTLIndexType c,
            IntPtr d,
            UIntPtr e,
            UIntPtr f,
            IntPtr g,
            UIntPtr h);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(
            IntPtr receiver,
            Selector selector,
            MTLPrimitiveType a,
            MTLIndexType b,
            MTLBuffer c,
            UIntPtr d,
            MTLBuffer e,
            UIntPtr f);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(
            IntPtr receiver,
            Selector selector,
            MTLBuffer a,
            UIntPtr b,
            MTLBuffer c,
            UIntPtr d,
            UIntPtr e);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(
            IntPtr receiver,
            Selector selector,
            IntPtr a,
            UIntPtr b,
            UIntPtr c,
            UIntPtr d,
            MTLSize e,
            IntPtr f,
            UIntPtr g,
            UIntPtr h,
            MTLOrigin i);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(
            IntPtr receiver,
            Selector selector,
            MTLRegion a,
            UIntPtr b,
            UIntPtr c,
            IntPtr d,
            UIntPtr e,
            UIntPtr f);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(
            IntPtr receiver,
            Selector selector,
            MTLTexture a,
            UIntPtr b,
            UIntPtr c,
            MTLOrigin d,
            MTLSize e,
            MTLBuffer f,
            UIntPtr g,
            UIntPtr h,
            UIntPtr i);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern byte* bytePtr_objc_msgSend(IntPtr receiver, Selector selector);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern CGSize CGSize_objc_msgSend(IntPtr receiver, Selector selector);


        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern byte byte_objc_msgSend(IntPtr receiver, Selector selector);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern Bool8 bool8_objc_msgSend(IntPtr receiver, Selector selector);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern Bool8 bool8_objc_msgSend(IntPtr receiver, Selector selector, UIntPtr a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern Bool8 bool8_objc_msgSend(IntPtr receiver, Selector selector, uint a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern uint uint_objc_msgSend(IntPtr receiver, Selector selector);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern float float_objc_msgSend(IntPtr receiver, Selector selector);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern double double_objc_msgSend(IntPtr receiver, Selector selector);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, IntPtr a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, IntPtr a, out NSError error);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, uint a, uint b, NSRange c, NSRange d);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, MTLComputePipelineDescriptor a, uint b, IntPtr c, out NSError error);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, uint a);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, UIntPtr a);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, IntPtr a, IntPtr b, out NSError error);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, IntPtr a, UIntPtr b);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, UIntPtr b, MTLResourceOptions c);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, void* a, UIntPtr b, MTLResourceOptions c);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern UIntPtr UIntPtr_objc_msgSend(IntPtr receiver, Selector selector);

        public static T objc_msgSend<T>(IntPtr receiver, Selector selector) where T : struct
        {
            IntPtr value = IntPtr_objc_msgSend(receiver, selector);
            return Unsafe.AsRef<T>(&value);
        }
        public static T objc_msgSend<T>(IntPtr receiver, Selector selector, IntPtr a) where T : struct
        {
            IntPtr value = IntPtr_objc_msgSend(receiver, selector, a);
            return Unsafe.AsRef<T>(&value);
        }
        public static string string_objc_msgSend(IntPtr receiver, Selector selector)
        {
            return objc_msgSend<NSString>(receiver, selector).GetValue();
        }

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, byte b);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, Bool8 b);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, uint b);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, float a, float b, float c, float d);
        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, IntPtr b);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
        public static extern void objc_msgSend_stret(void* retPtr, IntPtr receiver, Selector selector);
        public static T objc_msgSend_stret<T>(IntPtr receiver, Selector selector) where T : struct
        {
            T ret = default(T);
            objc_msgSend_stret(Unsafe.AsPointer(ref ret), receiver, selector);
            return ret;
        }

        [DllImport(ObjCLibrary)]
        public static extern IntPtr sel_registerName(byte* namePtr);

        [DllImport(ObjCLibrary)]
        public static extern byte* sel_getName(IntPtr selector);

        [DllImport(ObjCLibrary)]
        public static extern IntPtr objc_getClass(byte* namePtr);

        [DllImport(ObjCLibrary)]
        public static extern ObjCClass object_getClass(IntPtr obj);

        [DllImport(ObjCLibrary)]
        public static extern IntPtr class_getProperty(ObjCClass cls, byte* namePtr);

        [DllImport(ObjCLibrary)]
        public static extern byte* property_copyAttributeValue(IntPtr property, byte* attributeNamePtr);

        [DllImport(ObjCLibrary)]
        public static extern Selector method_getName(ObjectiveCMethod method);

        [DllImport(ObjCLibrary)]
        public static extern ObjectiveCMethod* class_copyMethodList(ObjCClass cls, out uint outCount);

        [DllImport(ObjCLibrary)]
        public static extern void free(IntPtr receiver);
        public static void retain(IntPtr receiver) => objc_msgSend(receiver, "retain");
        public static void release(IntPtr receiver) => objc_msgSend(receiver, "release");
        public static ulong GetRetainCount(IntPtr receiver) => (ulong)UIntPtr_objc_msgSend(receiver, "retainCount");
    }

    public unsafe struct ObjCClass
    {
        public readonly IntPtr NativePtr;
        public static implicit operator IntPtr(ObjCClass c) => c.NativePtr;

        public ObjCClass(string name)
        {
            int byteCount = Encoding.UTF8.GetMaxByteCount(name.Length);
            byte* utf8BytesPtr = stackalloc byte[byteCount];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, utf8BytesPtr, byteCount);
            }

            NativePtr = ObjectiveCRuntime.objc_getClass(utf8BytesPtr);
        }

        public IntPtr GetProperty(string propertyName)
        {
            int byteCount = Encoding.UTF8.GetMaxByteCount(propertyName.Length);
            byte* utf8BytesPtr = stackalloc byte[byteCount];
            fixed (char* namePtr = propertyName)
            {
                Encoding.UTF8.GetBytes(namePtr, propertyName.Length, utf8BytesPtr, byteCount);
            }

            return ObjectiveCRuntime.class_getProperty(this, utf8BytesPtr);
        }

        public T Alloc<T>() where T : struct
        {
            IntPtr value = ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, "alloc");
            return Unsafe.AsRef<T>(&value);
        }

        public T AllocInit<T>() where T : struct
        {
            IntPtr value = ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, "alloc");
            ObjectiveCRuntime.objc_msgSend(value, "init");
            return Unsafe.AsRef<T>(&value);
        }

        public ObjectiveCMethod* class_copyMethodList(out uint count)
        {
            return ObjectiveCRuntime.class_copyMethodList(this, out count);
        }
    }
}