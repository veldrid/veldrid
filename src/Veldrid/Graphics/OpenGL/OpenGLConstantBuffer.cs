using System;
using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLConstantBuffer : OpenGLBuffer, ConstantBuffer
    {
        public OpenGLConstantBuffer() : base(BufferTarget.UniformBuffer) { }

        public OpenGLConstantBuffer(ConstantBufferDataProvider dataProvider) : base(BufferTarget.UniformBuffer)
        {
            dataProvider.SetData(this);
        }

        public void SetData<T>(ref T data, int dataSizeInBytes) where T : struct
        {
            Bind();
            GL.BufferData(BufferTarget.UniformBuffer, dataSizeInBytes, ref data, BufferUsageHint.DynamicDraw);
            Unbind();
        }

        internal void BindToBlock(int program, int uniformBlockIndex, int dataSize, int uniformBindingIndex)
        {
            Bind();
            GL.BindBufferRange(BufferRangeTarget.UniformBuffer, uniformBindingIndex, BufferID, IntPtr.Zero, dataSize);
            GL.UniformBlockBinding(program, uniformBlockIndex, uniformBindingIndex);
        }
    }
}
