using System;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Diagnostics;

namespace Veldrid.OpenGL
{
    internal unsafe abstract class OpenGLBuffer : Buffer, OpenGLDeferredResource
    {
        private uint _buffer;
        private bool _dynamic;

        private string _name;
        private bool _nameChanged;
        public string Name { get => _name; set { _name = value; _nameChanged = true; } }

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
            if (_nameChanged)
            {
                _nameChanged = false;
                SetObjectLabel(ObjectLabelIdentifier.Buffer, _buffer, _name);
            }
        }

        public void CreateGLResources()
        {
            Debug.Assert(!Created);
            glGenBuffers(1, out _buffer);
            CheckLastError();

            glBindBuffer(Target, _buffer);
            CheckLastError();

            glBufferData(Target, (UIntPtr)SizeInBytes, null, _dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
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
