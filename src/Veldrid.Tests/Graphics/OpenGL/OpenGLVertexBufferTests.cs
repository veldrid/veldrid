using System;
using System.Linq;
using Veldrid.Platform;
using Xunit;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLVertexBufferTests
    {
        private OpenGLRenderContext _context;
        private OpenGLResourceFactory _factory;

        public OpenGLVertexBufferTests()
        {
            _context = TestData.CreateDefaultOpenGLRenderContext(TestData.CreateTestWindow());
            _factory = (OpenGLResourceFactory)_context.ResourceFactory;
        }

        [Fact]
        public void SetAndGet_Array()
        {
            OpenGLVertexBuffer vb = (OpenGLVertexBuffer)_factory.CreateVertexBuffer(1, false);

            float[] vertexData = Enumerable.Range(0, 150).Select(i => (float)i).ToArray();
            vb.SetVertexData(vertexData, new VertexDescriptor(4, 1, 0, IntPtr.Zero));

            float[] returned = new float[vertexData.Length];
            vb.GetData(returned);
            Assert.Equal(vertexData, returned);
        }

        [Fact]
        public void SetAndGet_Array_Offset()
        {
            OpenGLVertexBuffer vb = (OpenGLVertexBuffer)_factory.CreateVertexBuffer(1, false);

            float[] vertexData = Enumerable.Range(0, 150).Select(i => (float)i).ToArray();
            vb.SetVertexData(vertexData, new VertexDescriptor(sizeof(float), 1, 0, IntPtr.Zero), 250);

            float[] returned = new float[vertexData.Length + 250];
            vb.GetData(returned);
            for (int i = 250; i < returned.Length; i++)
            {
                Assert.Equal(vertexData[i - 250], returned[i]);
            }
        }

        [Fact]
        public unsafe void SetAndGet_IntPtr_Offset()
        {
            OpenGLVertexBuffer vb = (OpenGLVertexBuffer)_factory.CreateVertexBuffer(1, false);

            float[] vertexData = Enumerable.Range(0, 150).Select(i => (float)i).ToArray();
            fixed (float* dataPtr = vertexData)
            {
                vb.SetVertexData(new IntPtr(dataPtr), new VertexDescriptor(sizeof(float), 1, 0, IntPtr.Zero), 150, 250);
            }
            float[] returned = new float[vertexData.Length + 250];
            vb.GetData(returned);

            for (int i = 250; i < returned.Length; i++)
            {
                Assert.Equal(vertexData[i - 250], returned[i]);
            }
        }
    }
}
