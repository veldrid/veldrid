using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLBuffer : DeviceBuffer, IDisposable
    {
        private readonly BufferTarget _target;
        private readonly BufferUsageHint _bufferUsage;
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
        }

        public void SetData<T>(T[] data, int dataSizeInBytes) where T : struct
            => SetData(data, dataSizeInBytes, 0);
        public void SetData<T>(T[] data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            SetArrayDataCore(data, 0, dataSizeInBytes, destinationOffsetInBytes);
        }

        public void SetData<T>(ArraySegment<T> data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            SetArrayDataCore(data.Array, data.Offset, dataSizeInBytes, destinationOffsetInBytes);
        }

        private unsafe void SetArrayDataCore<T>(T[] data, int startIndex, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr sourceAddress = new IntPtr((byte*)handle.AddrOfPinnedObject().ToPointer() + (startIndex * Unsafe.SizeOf<T>()));
            SetData(sourceAddress, dataSizeInBytes, destinationOffsetInBytes);
            handle.Free();
        }

        public void SetData(IntPtr data, int dataSizeInBytes) => SetData(data, dataSizeInBytes, 0);
        public unsafe void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            Bind();
            EnsureBufferSize(dataSizeInBytes + destinationOffsetInBytes);
            GL.BufferSubData(_target, new IntPtr(destinationOffsetInBytes), dataSizeInBytes, data);
            Unbind();
        }

        public void GetData<T>(T[] storageLocation, int storageSizeInBytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(storageLocation, GCHandleType.Pinned);
            GetData(handle.AddrOfPinnedObject(), storageSizeInBytes);
            handle.Free();
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
            IntPtr mappedPtr = GL.MapBuffer(_target, BufferAccess.ReadOnly);
            SharpDX.Utilities.CopyMemory(storageLocation, mappedPtr, bytesToCopy);
            if (!GL.UnmapBuffer(_target))
            {
                throw new InvalidOperationException("UnmapBuffer failed.");
            }
            Unbind();
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
                throw new InvalidOperationException($"{_target} {_bufferID} not uploaded correctly. Expected:{expectedSizeInBytes}, Actual:{bufferSize}");
            }
#endif
        }

        public IntPtr MapBuffer(int numBytes)
        {
            EnsureBufferSize(numBytes);
            return GL.MapBuffer(_target, BufferAccess.WriteOnly);
        }

        public void UnmapBuffer()
        {
            if (GL.UnmapBuffer(_target))
            {
                throw new InvalidOperationException("GL.UnmapBuffer failed.");
            }
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_bufferID);
        }
    }
}