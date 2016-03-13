using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLBuffer : DeviceBuffer, IDisposable
    {
        private readonly BufferTarget _target;
        private int _bufferID;
        private int _bufferSize;

        public OpenGLBuffer(BufferTarget target)
        {
            _bufferID = GL.GenBuffer();
            _target = target;
            _bufferSize = 0;
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
            EnsureBufferSize(dataSizeInBytes);
            GL.BufferData(_target, dataSizeInBytes, ref data, BufferUsageHint.DynamicDraw);
            Unbind();
        }

        public void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(T[] data, int dataSizeInBytes) where T : struct
        {
            Bind();
            EnsureBufferSize(dataSizeInBytes);
            GL.BufferData(_target, dataSizeInBytes, data, BufferUsageHint.DynamicDraw);
            Unbind();

            ValidateBufferSize(dataSizeInBytes);
        }

        public void SetData<T>(T[] data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            Bind();
            EnsureBufferSize(dataSizeInBytes);
            GL.BufferSubData(_target, new IntPtr(destinationOffsetInBytes), dataSizeInBytes, data);
            Unbind();

            ValidateBufferSize(dataSizeInBytes);
        }

        public void SetData(IntPtr data, int dataSizeInBytes)
        {
            Bind();
            EnsureBufferSize(dataSizeInBytes);
            GL.BufferData(_target, dataSizeInBytes, data, BufferUsageHint.DynamicDraw);
            Unbind();

            ValidateBufferSize(dataSizeInBytes);
        }

        public void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            Bind();
            EnsureBufferSize(dataSizeInBytes);
            GL.BufferSubData(_target, new IntPtr(destinationOffsetInBytes), dataSizeInBytes, data);
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

        public void GetData(IntPtr storageLocation, int storageSizeInBytes)
        {
            GL.GetBufferSubData(_target, IntPtr.Zero, storageSizeInBytes, storageLocation);
        }

        private void EnsureBufferSize(int dataSizeInBytes)
        {
            if (_bufferSize < dataSizeInBytes)
            {
                int oldBuffer = _bufferID;
                _bufferID = GL.GenBuffer();
                GL.BindBuffer(_target, _bufferID);
                GL.BufferData(_target, dataSizeInBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.CopyNamedBufferSubData(oldBuffer, _bufferID, IntPtr.Zero, IntPtr.Zero, _bufferSize);
                GL.DeleteBuffer(oldBuffer);
                _bufferSize = dataSizeInBytes;
            }
        }
    }
}