using System;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Diagnostics;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLBuffer : Buffer, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private uint _buffer;
        private bool _dynamic;

        private string _name;
        private bool _nameChanged;
        public string Name { get => _name; set { _name = value; _nameChanged = true; } }

        public override uint SizeInBytes { get; }
        public override BufferUsage Usage { get; }

        public uint Buffer => _buffer;

        public bool Created { get; private set; }

        public OpenGLBuffer(OpenGLGraphicsDevice gd, uint sizeInBytes, BufferUsage usage)
        {
            _gd = gd;
            SizeInBytes = sizeInBytes;
            _dynamic = (usage & BufferUsage.Dynamic) == BufferUsage.Dynamic;
            Usage = usage;
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

            // Bind to a target not used anywhere else
            // TODO: Make this more robust.
            glBindBuffer(BufferTarget.TextureBuffer, _buffer);
            CheckLastError();

            glBufferData(
                BufferTarget.TextureBuffer,
                (UIntPtr)SizeInBytes,
                null,
                _dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
            CheckLastError();

            Created = true;
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
        }

        public void DestroyGLResources()
        {
            uint buffer = _buffer;
            glDeleteBuffers(1, ref buffer);
            CheckLastError();
        }
    }
}
