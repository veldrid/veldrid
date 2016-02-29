using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLVertexBuffer : OpenGLBuffer, VertexBuffer
    {
        private VertexDescriptor _descriptor;

        public OpenGLVertexBuffer() : base(BufferTarget.ArrayBuffer) { }

        public void Apply()
        {
            Bind();
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct
        {
            _descriptor = descriptor;

            Bind();
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertexData.Length * descriptor.VertexSizeInBytes,
                vertexData,
                BufferUsageHint.DynamicDraw);

#if DEBUG
            // Validate that the buffer is the correct size
            int bufferSize;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            if (vertexData.Length * descriptor.VertexSizeInBytes != bufferSize)
            {
                throw new InvalidOperationException("Vertex array not uploaded correctly");
            }
#endif
        }
    }


}
