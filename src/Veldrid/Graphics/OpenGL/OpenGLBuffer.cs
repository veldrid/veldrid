using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLBuffer : DeviceBuffer, IDisposable
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

        public void SetData<T>(ref T data, int dataSizeInBytes) where T : struct
        {
            Bind();
            GL.BufferData(_target, dataSizeInBytes, ref data, BufferUsageHint.DynamicDraw);
            Unbind();
        }

        public void SetData<T>(T[] data, int dataSizeInBytes) where T : struct
        {
            Bind();
            GL.BufferData(_target, dataSizeInBytes, data, BufferUsageHint.DynamicDraw);
            Unbind();

            ValidateBufferSize(dataSizeInBytes);
        }

        [Conditional("Debug")]
        private void ValidateBufferSize(int expectedSizeInBytes)
        {
            int bufferSize;
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            if (expectedSizeInBytes * sizeof(int) != bufferSize)
            {
                throw new InvalidOperationException("Vertex array not uploaded correctly");
            }
        }

        public void GetData<T>(T[] storageLocation, int storageSizeInBytes) where T : struct
        {
            GL.GetBufferSubData(_target, IntPtr.Zero, storageSizeInBytes, storageLocation);
        }

        public void GetData<T>(ref T storageLocation, int storageSizeInBytes) where T : struct
        {
            GL.GetBufferSubData(_target, IntPtr.Zero, storageSizeInBytes, ref storageLocation);
        }
    }
}