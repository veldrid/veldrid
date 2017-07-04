using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void SetThenGetIntPtr<T>(DeviceBuffer db, T[] data, int offsetInElements)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            int sizeOfElement = Unsafe.SizeOf<T>();
            int dataCount = ((Array)data).Length;
            SetThenGetIntPtrTest(db, handle.AddrOfPinnedObject(), dataCount, sizeOfElement, offsetInElements);
            handle.Free();
        }

        public static unsafe void SetThenGetIntPtrTest(
            DeviceBuffer db,
            IntPtr data,
            int dataCount,
            int sizeOfElement,
            int offsetInElements)
        {
            db.SetData(data, dataCount * sizeOfElement, offsetInElements * sizeOfElement);
            int retSize = dataCount * sizeOfElement + offsetInElements * sizeOfElement;
            byte* retPtr = stackalloc byte[retSize];
            db.GetData(new IntPtr(retPtr), retSize);
            byte* dataPtr = (byte*)data.ToPointer();
            for (int i = 0; i < dataCount; i++)
            {
                Assert.Equal(dataPtr[i], retPtr[i + offsetInElements * sizeOfElement]);
            }
        }
    }
}
