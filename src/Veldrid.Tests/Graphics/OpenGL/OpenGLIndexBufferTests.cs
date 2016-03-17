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
            _context = new OpenGLRenderContext(new TestWindow());
            _factory = new OpenGLResourceFactory();
        }

        [Fact]
        public void SetAndGet_Array()
        {
            OpenGLIndexBuffer ib = (OpenGLIndexBuffer)_factory.CreateIndexBuffer(1, false);

            int[] indexData = Enumerable.Range(0, 150).ToArray();
            ib.SetIndices(indexData);

            int[] returned = new int[indexData.Length];
            ib.GetData(returned, returned.Length * 4);
            Assert.Equal(indexData, returned);
        }

        [Fact]
        public unsafe void SetAndGet_IntPtr_Offset()
        {
            OpenGLIndexBuffer ib = (OpenGLIndexBuffer)_factory.CreateIndexBuffer(1, false);

            ushort[] indexData = Enumerable.Range(0, 150).Select(i => (ushort)i).ToArray();
            fixed (ushort* dataPtr = indexData)
            {
                ib.SetIndices(new IntPtr(dataPtr), IndexFormat.UInt16, 2, 150, 250);
            }
            ushort[] returned = new ushort[indexData.Length + 250];
            ib.GetData(returned, returned.Length * 4);
            for (int i = 0; i < 250; i++)
            {
                Assert.Equal(0, returned[i]);
            }
            for (int i = 250; i < returned.Length; i++)
            {
                Assert.Equal(indexData[i - 250], returned[i]);
            }
        }
    }
}
