using System;
using System.Runtime.InteropServices;
using System.Linq;
using Veldrid.Platform;
using Xunit;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DVertexBufferTests
    {
        private D3DRenderContext _context;
        private D3DResourceFactory _factory;

        public D3DVertexBufferTests()
        {
            _context = new D3DRenderContext(TestData.CreateTestWindow());
            _factory = new D3DResourceFactory(_context.Device);
        }

        [Fact]
        public void SetAndGet_Array()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new Exception("Why is this test running");
            }
            D3DVertexBuffer vb = (D3DVertexBuffer)_factory.CreateVertexBuffer(1, false);

            float[] vertexData = Enumerable.Range(0, 150).Select(i => (float)i).ToArray();
            vb.SetVertexData(vertexData, new VertexDescriptor(sizeof(float), 1, 0, IntPtr.Zero));

            float[] returned = new float[vertexData.Length];
            vb.GetData(returned);
            Assert.Equal(vertexData, returned);
        }

        [Fact]
        public void SetAndGet_Array_Offset()
        {
            D3DVertexBuffer vb = (D3DVertexBuffer)_factory.CreateVertexBuffer(1, false);

            float[] vertexData = Enumerable.Range(0, 150).Select(i => (float)i).ToArray();
            vb.SetVertexData(vertexData, new VertexDescriptor(sizeof(float), 1, 0, IntPtr.Zero), 250);

            float[] returned = new float[vertexData.Length + 250];
            vb.GetData(returned);
            for (int i = 0; i < 250; i++)
            {
                Assert.Equal(0, returned[i]);
            }
            for (int i = 250; i < returned.Length; i++)
            {
                Assert.Equal(vertexData[i - 250], returned[i]);
            }
        }

        [Fact]
        public unsafe void SetAndGet_IntPtr_Offset()
        {
            D3DVertexBuffer vb = (D3DVertexBuffer)_factory.CreateVertexBuffer(1, false);

            float[] vertexData = Enumerable.Range(0, 150).Select(i => (float)i).ToArray();
            fixed (float* dataPtr = vertexData)
            {
                vb.SetVertexData(new IntPtr(dataPtr), new VertexDescriptor(4, 1, 0, IntPtr.Zero), 150, 250);
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
