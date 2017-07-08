using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.CompilerServices;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLBuffer : DeviceBufferBase, IDisposable
    {
        private readonly BufferTarget _target;
        private readonly BufferUsageHint _bufferUsage;
        private int _bufferID;
        private int _bufferSize;

        public OpenGLBuffer(BufferTarget target) : this(target, 0, BufferUsageHint.DynamicDraw) { }
        public OpenGLBuffer(BufferTarget target, int sizeInBytes) : this(target, sizeInBytes, BufferUsageHint.DynamicDraw) { }
        public OpenGLBuffer(BufferTarget target, int sizeInBytes, BufferUsageHint bufferUsage)
        {
            _bufferID = GL.GenBuffer();
            _bufferUsage = bufferUsage;
            _target = target;
            Bind();
            EnsureBufferSize(sizeInBytes);
            Unbind();
        }

        public int BufferSize => _bufferSize;

        public int BufferID => _bufferID;

        protected void Bind()
        {
            GL.BindBuffer(_target, _bufferID);
        }

        protected void Unbind()
        {
            GL.BindBuffer(_target, 0);
        }

        public unsafe override void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            Bind();
            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);
            GL.BufferSubData(_target, new IntPtr(destinationOffsetInBytes), dataSizeInBytes, data);
        }

        public unsafe override void GetData(IntPtr storageLocation, int storageSizeInBytes)
        {
            int bytesToCopy = Math.Min(_bufferSize, storageSizeInBytes);
            Bind();
            IntPtr mappedPtr = GL.MapBuffer(_target, BufferAccess.ReadOnly);
            Unsafe.CopyBlock(storageLocation.ToPointer(), mappedPtr.ToPointer(), (uint)bytesToCopy);
            if (!GL.UnmapBuffer(_target))
            {
                throw new VeldridException("UnmapBuffer failed.");
            }
        }

        private void EnsureBufferSize(int dataSizeInBytes)
        {
            // Buffer must be bound already.
            if (_bufferSize < dataSizeInBytes)
            {
                int oldBufferID = _bufferID;
                _bufferID = GL.GenBuffer();
                GL.BindBuffer(_target, _bufferID);
                GL.BufferData(_target, dataSizeInBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                if (_bufferSize > 0)
                {
                    GL.BindBuffer(BufferTarget.CopyReadBuffer, oldBufferID);
                    GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, _target, IntPtr.Zero, IntPtr.Zero, _bufferSize);
                }
                _bufferSize = dataSizeInBytes;
                GL.DeleteBuffer(oldBufferID);

                ValidateBufferSize(dataSizeInBytes);
            }
        }

        private void ValidateBufferSize(int expectedSizeInBytes)
        {
#if DEBUG
            int bufferSize;
            GL.GetBufferParameter(_target, BufferParameterName.BufferSize, out bufferSize);
            if (expectedSizeInBytes != bufferSize)
            {
                throw new VeldridException($"{_target} {_bufferID} not uploaded correctly. Expected:{expectedSizeInBytes}, Actual:{bufferSize}");
            }
#endif
        }

        public override IntPtr MapBuffer(int numBytes)
        {
            EnsureBufferSize(numBytes);
            return GL.MapBuffer(_target, BufferAccess.WriteOnly);
        }

        public override void UnmapBuffer()
        {
            if (!GL.UnmapBuffer(_target))
            {
                throw new VeldridException("GL.UnmapBuffer failed.");
            }
        }

        public override void Dispose()
        {
            GL.DeleteBuffer(_bufferID);
        }
    }
}