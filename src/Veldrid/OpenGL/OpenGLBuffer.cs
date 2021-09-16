using System;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Diagnostics;
using System.Threading;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLBuffer : DeviceBuffer, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private uint _buffer;
        private bool _disposeRequested;

        public int DestructionCount = 0;
        public int ConstructionCount = 0;

        private string? _name;
        private bool _nameChanged;

        public override string? Name { get => _name; set { _name = value; _nameChanged = true; } }

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

            BufferStorageMask storageMask = BufferStorageMask.DynamicStorage;

            if ((Usage & BufferUsage.StagingRead) != 0 ||
                (Usage & BufferUsage.DynamicRead) != 0)
            {
                storageMask |= BufferStorageMask.MapRead;
                storageMask |= BufferStorageMask.ClientStorage;
            }

            if ((Usage & BufferUsage.StagingWrite) != 0 ||
                (Usage & BufferUsage.DynamicWrite) != 0)
            {
                storageMask |= BufferStorageMask.MapWrite;
                storageMask |= BufferStorageMask.ClientStorage;
            }

            BufferUsageHint hint;
            if ((Usage & BufferUsage.StagingRead) != 0)
            {
                hint = BufferUsageHint.StreamRead;
            }
            else if ((Usage & BufferUsage.StagingWrite) != 0)
            {
                hint = BufferUsageHint.StreamCopy;
            }
            else if ((Usage & BufferUsage.DynamicRead) != 0)
            {
                hint = BufferUsageHint.DynamicRead;
            }
            else if ((Usage & BufferUsage.DynamicWrite) != 0)
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

                if (_gd.Extensions.ARB_buffer_storage)
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
                uint buffer;
                glGenBuffers(1, &buffer);
                CheckLastError();
                _buffer = buffer;

                glBindBuffer(BufferTarget.CopyReadBuffer, _buffer);
                CheckLastError();

                if (_gd.Extensions.ARB_buffer_storage)
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

            Interlocked.Increment(ref ConstructionCount);
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
            glDeleteBuffers(1, &buffer);
            CheckLastError();
            _buffer = buffer;

            Interlocked.Increment(ref DestructionCount);
        }
    }
}
