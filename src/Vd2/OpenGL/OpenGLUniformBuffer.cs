namespace Vd2.OpenGL
{
    internal class OpenGLUniformBuffer : OpenGLBuffer, UniformBuffer
    {
        public OpenGLUniformBuffer(ref BufferDescription description)
            : base(description.SizeInBytes, description.Dynamic)
        {
        }
    }
}
