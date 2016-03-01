using System;
using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLIndexBuffer : OpenGLBuffer, IndexBuffer, IDisposable
    {
        private int _stride;
        private IntPtr _offset;

        public OpenGLIndexBuffer() : base(BufferTarget.ElementArrayBuffer)
        {
            _stride = 0;
            _offset = IntPtr.Zero;
        }

        public void Apply()
        {
            Bind();
        }

        public void SetIndices(int[] indices) => SetIndices(indices, 0, IntPtr.Zero);

        public void SetIndices(int[] indices, int stride, IntPtr offset)
        {
            UpdateBufferData(indices, sizeof(int) * indices.Length);

            _stride = stride;
            _offset = offset;
        }
    }
}
