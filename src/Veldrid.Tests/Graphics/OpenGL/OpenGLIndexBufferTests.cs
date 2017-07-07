using System;
using System.Linq;
using Veldrid.Platform;
using Xunit;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLIndexBufferTests
    {
        private OpenGLRenderContext _context;
        private OpenGLResourceFactory _factory;

        public OpenGLIndexBufferTests()
        {
            _context = TestData.CreateDefaultOpenGLRenderContext(TestData.CreateTestWindow());
            _factory = (OpenGLResourceFactory)_context.ResourceFactory;
        }

        [Fact]
        public void SetAndGet_Array()
        {
            OpenGLIndexBuffer ib = (OpenGLIndexBuffer)_factory.CreateIndexBuffer(1, false);

            uint[] indexData = Enumerable.Range(0, 150).Select(i => (uint)i).ToArray();
            ib.SetIndices(indexData);

            uint[] returned = new uint[indexData.Length];
            ib.GetData(returned);
            Assert.Equal(indexData, returned);
        }

        [Fact]
        public unsafe void SetAndGet_IntPtr_Offset()
        {
            OpenGLIndexBuffer ib = (OpenGLIndexBuffer)_factory.CreateIndexBuffer(1, false);

            ushort[] indexData = Enumerable.Range(0, 150).Select(i => (ushort)i).ToArray();
            fixed (ushort* dataPtr = indexData)
            {
                ib.SetIndices(new IntPtr(dataPtr), IndexFormat.UInt16, 150, 250);
            }
            ushort[] returned = new ushort[indexData.Length + 250];
            ib.GetData(returned);

            for (int i = 250; i < returned.Length; i++)
            {
                Assert.Equal(indexData[i - 250], returned[i]);
            }
        }
    }
}
