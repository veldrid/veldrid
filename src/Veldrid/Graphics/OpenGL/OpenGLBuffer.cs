using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLBuffer : DeviceBuffer, IDisposable
    {
        private readonly BufferTarget _target;
        private readonly BufferUsageHint _bufferUsage;
        private readonly bool _isDynamic;
        private int _bufferID;
        private int _bufferSize;

        public OpenGLBuffer(BufferTarget target) : this(target, BufferUsageHint.DynamicDraw) { }
        public OpenGLBuffer(BufferTarget target, BufferUsageHint bufferUsage)
        {
            _bufferID = GL.GenBuffer();
            _bufferUsage = bufferUsage;
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

        public void SetData<T>(ref T data, int dataSizeInBytes) where T : struct
            => SetData(ref data, dataSizeInBytes, 0);
        public void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            Bind();
            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);
            GL.BufferSubData(_target, new IntPtr(destinationOffsetInBytes), dataSizeInBytes, ref data);
            Unbind();

            ValidateBufferSize(dataSizeInBytes);
        }

        public void SetData<T>(T[] data, int dataSizeInBytes) where T : struct
            => SetData(data, dataSizeInBytes, 0);
        public void SetData<T>(T[] data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            Bind();
            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);
            GL.BufferSubData(_target, new IntPtr(destinationOffsetInBytes), dataSizeInBytes, data);
            Unbind();

            ValidateBufferSize(dataSizeInBytes);
        }

        public void SetData(IntPtr data, int dataSizeInBytes) => SetData(data, dataSizeInBytes, 0);
        public unsafe void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            Bind();
            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);
            GL.BufferSubData(_target, new IntPtr(destinationOffsetInBytes), dataSizeInBytes, data);
            Unbind();

            ValidateBufferSize(dataSizeInBytes);
        }

        [Conditional("Debug")]
        private void ValidateBufferSize(int expectedSizeInBytes)
        {
            Bind();
            int bufferSize;
            GL.GetBufferParameter(_target, BufferParameterName.BufferSize, out bufferSize);
            if (expectedSizeInBytes * sizeof(int) != bufferSize)
            {
                throw new InvalidOperationException("Vertex array not uploaded correctly");
            }
            Unbind();
        }


        public void GetData<T>(T[] storageLocation, int storageSizeInBytes) where T : struct
        {
            int bytesToCopy = Math.Min(_bufferSize, storageSizeInBytes);
            Bind();
            GL.GetBufferSubData(_target, IntPtr.Zero, bytesToCopy, storageLocation);
            Unbind();
        }

        public void GetData<T>(ref T storageLocation, int storageSizeInBytes) where T : struct
        {
            int bytesToCopy = Math.Min(_bufferSize, storageSizeInBytes);
            Bind();
            GL.GetBufferSubData(_target, IntPtr.Zero, bytesToCopy, ref storageLocation);
            Unbind();
        }

        public void GetData(IntPtr storageLocation, int storageSizeInBytes)
        {
            int bytesToCopy = Math.Min(_bufferSize, storageSizeInBytes);
            Bind();
            GL.GetBufferSubData(_target, IntPtr.Zero, bytesToCopy, storageLocation);
            Unbind();
        }

        private void EnsureBufferSize(int dataSizeInBytes)
        {
            // Buffer must be bound already.
            if (_bufferSize < dataSizeInBytes)
            {
                GL.BufferData(_target, dataSizeInBytes, IntPtr.Zero, _bufferUsage);
                _bufferSize = dataSizeInBytes;
            }
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_bufferID);
        }
    }
}