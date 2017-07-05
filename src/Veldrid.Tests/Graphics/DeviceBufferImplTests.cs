using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace Veldrid.Graphics
{
    public static class DeviceBufferImplTests
    {
        public static IEnumerable<object[]> SetAndGetData()
        {
            foreach (RenderContext rc in TestData.RenderContexts())
            {
                foreach (object data in TestData.DataValueArrays())
                {
                    foreach (int offset in new[] { 0, 1, 100 })
                    {
                        var factory = rc.ResourceFactory;
                        yield return TestData.Array(factory.CreateVertexBuffer(1, false), data, offset);
                        yield return TestData.Array(factory.CreateIndexBuffer(1, false), data, offset);
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(SetAndGetData))]
        public static void SetThenGetGeneric<T>(DeviceBuffer db, T[] data, int offsetInElements) where T : struct
        {
            int numElements = data.Length + offsetInElements;
            db.SetData(data, offsetInElements * Unsafe.SizeOf<T>());
            T[] ret = new T[numElements];
            db.GetData(ret);
            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(data[i], ret[i + offsetInElements]);
            }
        }

        [Theory]
        [MemberData(nameof(SetAndGetData))]
        public unsafe static void SetThenGetIntPtr<T>(DeviceBuffer db, T[] data, int offsetInElements)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            int sizeOfElement = Unsafe.SizeOf<T>();
            int dataCount = data.Length;
            db.SetData(handle.AddrOfPinnedObject(), dataCount * sizeOfElement, offsetInElements * sizeOfElement);
            int retSize = dataCount * sizeOfElement + offsetInElements * sizeOfElement;
            byte* retPtr = stackalloc byte[retSize];
            db.GetData(new IntPtr(retPtr), retSize);
            byte* dataPtr = (byte*)handle.AddrOfPinnedObject().ToPointer();
            for (int i = 0; i < dataCount; i++)
            {
                Assert.Equal(dataPtr[i], retPtr[i + offsetInElements * sizeOfElement]);
            }
            handle.Free();
        }

        public static IEnumerable<object[]> ConstantBufferSetTestData()
        {
            foreach (RenderContext rc in TestData.RenderContexts())
            {
                foreach (object value in new object[]
                {
                    Matrix4x4.Identity,
                    Matrix4x4.CreatePerspective(1280, 720, 2, 500),
                    Vector4.One,
                    Quaternion.Identity,
                    new TestStruct
                    {
                        A  = Vector3.One,
                        B = new Vector4(5, 6, 7, 8),
                        C = new Vector3(-1, -2, -3), 
                        D = new Vector2(5, 6),
                        E = new Vector4(7, 8, 9, 10)
                    }
                })
                {
                    yield return TestData.Array(rc, value);
                }
            }
        }

        [Theory]
        [MemberData(nameof(ConstantBufferSetTestData))]
        public static void ConstantBufferSetThenGet(RenderContext rc, object value)
        {
            typeof(DeviceBufferImplTests).GetMethod("ConstantBufferSetGeneric", BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(value.GetType())
                 .Invoke(null, new[] { rc, value });
        }

        private static void ConstantBufferSetGeneric<T>(RenderContext rc, T value) where T : struct, IEquatable<T>
        {
            int sizeOfT = Unsafe.SizeOf<T>();
            ConstantBuffer cb = rc.ResourceFactory.CreateConstantBuffer(sizeOfT);
            cb.SetData(ref value, sizeOfT);
            T returnedValue = default(T);
            cb.GetData(ref returnedValue, sizeOfT);

            Assert.Equal(value, returnedValue);
        }


        private struct TestStruct : IEquatable<TestStruct>
        {
            public Vector3 A;
            public Vector4 B;
            public Vector3 C;
            public Vector2 D;
            public Vector4 E;

            public bool Equals(TestStruct other)
            {
                return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D) && E.Equals(other.E);
            }
        }
    }

}
