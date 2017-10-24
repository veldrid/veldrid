using System;
using static Vd2.OpenGLBinding.OpenGLNative;
using static Vd2.OpenGL.OpenGLUtil;
using Vd2.OpenGLBinding;
using System.Diagnostics;

namespace Vd2.OpenGL
{
    internal abstract class OpenGLBuffer : Buffer, OpenGLDeferredResource
    {
        private uint _buffer;

        public ulong SizeInBytes { get; }

        public uint Buffer => _buffer;

        public bool Created { get; private set; }

        public OpenGLBuffer(ulong sizeInBytes, bool dynamic)
        {
            SizeInBytes = sizeInBytes;
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
