using System;
using static Vd2.OpenGLBinding.OpenGLNative;
using static Vd2.OpenGL.OpenGLUtil;
using Vd2.OpenGLBinding;
using System.Diagnostics;

namespace Vd2.OpenGL
{
    internal unsafe abstract class OpenGLBuffer : Buffer, OpenGLDeferredResource
    {
        private uint _buffer;
        private bool _dynamic;

        public ulong SizeInBytes { get; }
        public BufferTarget Target { get; }

        public uint Buffer => _buffer;

        public bool Created { get; private set; }

        public OpenGLBuffer(ulong sizeInBytes, bool dynamic, BufferTarget target)
        {
            SizeInBytes = sizeInBytes;
            _dynamic = dynamic;
            Target = target;
        }

        public void EnsureResourcesCreated()
        {
            if (!Created)
            {
                CreateGLResources();
            }
        }

        public void CreateGLResources()
        {
            Debug.Assert(!Created);
            glGenBuffers(1, out _buffer);
            CheckLastError();

            glBindBuffer(Target, _buffer);
            CheckLastError();

            glNamedBufferData(_buffer, (uint)SizeInBytes, null, _dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
            CheckLastError();

            Created = true;
        }

        public void Dispose()
        {
        }

        public void DestroyGLResources()
        {
            uint buffer = _buffer;
            glDeleteBuffers(1, ref buffer);
            CheckLastError();
        }
    }
}
