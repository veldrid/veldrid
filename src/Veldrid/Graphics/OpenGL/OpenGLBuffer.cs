using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLBuffer : IDisposable
    {
        private readonly int _bufferID;
        private readonly BufferTarget _target;

        public OpenGLBuffer(BufferTarget target)
        {
            _bufferID = GL.GenBuffer();
            _target = target;
        }

        protected int BufferID => _bufferID;

        protected void Bind()
        {
            GL.BindBuffer(_target, _bufferID);
        }

        protected void Unbind()
        {
            GL.BindBuffer(_target, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_bufferID);
        }
    }
}