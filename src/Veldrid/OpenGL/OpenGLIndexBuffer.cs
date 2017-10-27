using Veldrid.OpenGLBinding;

namespace Veldrid.OpenGL
{
    internal class OpenGLIndexBuffer : OpenGLBuffer, IndexBuffer
    {
        public IndexFormat Format { get; }
        public DrawElementsType DrawElementsType { get; }

        public OpenGLIndexBuffer(ref IndexBufferDescription description)
            : base(description.SizeInBytes, description.Dynamic, BufferTarget.ElementArrayBuffer)
        {
            Format = description.Format;
            DrawElementsType = OpenGLFormats.VdToGLDrawElementsType(description.Format);
        }
    }
}
