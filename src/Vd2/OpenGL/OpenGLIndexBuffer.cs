using Vd2.OpenGLBinding;

namespace Vd2.OpenGL
{
    internal class OpenGLIndexBuffer : OpenGLBuffer, IndexBuffer
    {
        public IndexFormat Format { get; }
        public DrawElementsType DrawElementsType { get; }

        public OpenGLIndexBuffer(ref IndexBufferDescription description)
            : base(description.SizeInBytes, description.Dynamic)
        {
            Format = description.Format;
            DrawElementsType = OpenGLFormats.VdToGLDrawElementsType(description.Format);
        }
    }
}
