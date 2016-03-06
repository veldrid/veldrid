using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLVertexBuffer : OpenGLBuffer, VertexBuffer
    {
        public OpenGLVertexBuffer() : base(BufferTarget.ArrayBuffer) { }

        public void Apply()
        {
            Bind();
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct
        {
            SetData(vertexData, descriptor.VertexSizeInBytes * vertexData.Length);
        }
    }
}
