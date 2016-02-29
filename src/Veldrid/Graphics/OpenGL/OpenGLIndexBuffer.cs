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
            Bind();
            GL.BufferData(
                BufferTarget.ElementArrayBuffer, 
                sizeof(int) * indices.Length, 
                indices, 
                BufferUsageHint.StaticDraw);

            _stride = stride;
            _offset = offset;

#if DEBUG
            // Validate that the buffer is the correct size
            int bufferSize;
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            if (indices.Length * sizeof(int) != bufferSize)
            {
                throw new InvalidOperationException("Vertex array not uploaded correctly");
            }
#endif
        }
    }
}
