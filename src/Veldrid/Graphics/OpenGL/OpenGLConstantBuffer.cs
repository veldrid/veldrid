using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLConstantBuffer : OpenGLBuffer, ConstantBuffer
    {
        public OpenGLConstantBuffer(int sizeInBytes) : base(BufferTarget.UniformBuffer, sizeInBytes) { }

        public OpenGLConstantBuffer(ConstantBufferDataProvider dataProvider) : base(BufferTarget.UniformBuffer)
        {
            dataProvider.SetData(this);
        }
    }
}
