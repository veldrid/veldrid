using System;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Diagnostics;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLBuffer : DeviceBuffer, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private uint _buffer;
        private bool _disposeRequested;

        private string _name;
        private bool _nameChanged;

        public override string Name { get => _name; set { _name = value; _nameChanged = true; } }

        public override uint SizeInBytes { get; }
        public override BufferUsage Usage { get; }

        public uint Buffer => _buffer;

        public bool Created { get; private set; }

        public override bool IsDisposed => _disposeRequested;

        public OpenGLBuffer(OpenGLGraphicsDevice gd, uint sizeInBytes, BufferUsage usage, IntPtr initialData)
        {
            _gd = gd;
            SizeInBytes = sizeInBytes;
            Usage = usage;

            if (initialData != IntPtr.Zero)
            {
                gd.CreateBuffer(this, initialData);
            }
        }

        public void EnsureResourcesCreated()
        {
            if (!Created)
            {
                CreateGLResources(IntPtr.Zero);
            }
            if (_nameChanged)
            {
                _nameChanged = false;
                if (_gd.Extensions.KHR_Debug)
                {
                    SetObjectLabel(ObjectLabelIdentifier.Buffer, _buffer, _name);
                }
            }
        }

        public void CreateGLResources(IntPtr initialData)
        {
            Debug.Assert(!Created);

            BufferStorageMask storageMask =
                BufferStorageMask.ClientStorage |
                BufferStorageMask.DynamicStorage |
                BufferStorageMask.MapWrite |
                BufferStorageMask.MapRead;

            BufferUsageHint hint;
            if ((Usage & BufferUsage.Staging) == BufferUsage.Staging)
            {
                hint = BufferUsageHint.StreamCopy;
            }
            else if ((Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic)
            {
                hint = BufferUsageHint.DynamicDraw;
            }
            else
            {
                hint = BufferUsageHint.StaticDraw;
            }

            if (_gd.Extensions.ARB_DirectStateAccess)
            {
                uint buffer;
                glCreateBuffers(1, &buffer);
                CheckLastError();
                _buffer = buffer;

                if (hint == BufferUsageHint.StreamCopy &&
                    _gd.BackendType != GraphicsBackend.OpenGLES)
                {
                    glNamedBufferStorage(
                        _buffer,
                        SizeInBytes,
                        (void*)initialData,
                        storageMask);
                }
                else
                {
                    glNamedBufferData(
                        _buffer,
                        SizeInBytes,
                        (void*)initialData,
                        hint);
                }
                CheckLastError();
            }
            else
            {
                glGenBuffers(1, out _buffer);
                CheckLastError();

                glBindBuffer(BufferTarget.CopyReadBuffer, _buffer);
                CheckLastError();

                if (hint == BufferUsageHint.StreamCopy &&
                    _gd.BackendType != GraphicsBackend.OpenGLES )
                {
                    glBufferStorage(
                        BufferTarget.CopyReadBuffer,
                        SizeInBytes,
                        (void*)initialData,
                        storageMask);
                }
                else
                {
                    glBufferData(
                        BufferTarget.CopyReadBuffer,
                        SizeInBytes,
                        (void*)initialData,
                        hint);
                }
                CheckLastError();
            }

            Created = true;
        }

        public override void Dispose()
        {
            if (!_disposeRequested)
            {
                _disposeRequested = true;
                _gd.EnqueueDisposal(this);
            }
        }

        public void DestroyGLResources()
        {
            uint buffer = _buffer;
            glDeleteBuffers(1, ref buffer);
            CheckLastError();
        }
    }
}
