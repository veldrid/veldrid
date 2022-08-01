using System;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Diagnostics;

namespace Veldrid.OpenGL
{
    internal sealed unsafe class OpenGLBuffer : DeviceBuffer, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private uint _buffer;
        private bool _disposeRequested;

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

        private static BufferStorageMask GetStorageMask(BufferUsage usage)
        {
            BufferStorageMask storageMask =
                BufferStorageMask.DynamicStorage |
                BufferStorageMask.ClientStorage;

            if ((usage & BufferUsage.StagingRead) != 0 ||
                (usage & BufferUsage.DynamicRead) != 0)
            {
                storageMask &= ~BufferStorageMask.ClientStorage;
                storageMask |= BufferStorageMask.MapRead;
            }

            if ((usage & BufferUsage.StagingWrite) != 0 ||
                (usage & BufferUsage.DynamicWrite) != 0)
            {
                storageMask &= ~BufferStorageMask.ClientStorage;
                storageMask |= BufferStorageMask.MapWrite;
            }

            return storageMask;
        }

        private static BufferUsageHint GetUsageHint(BufferUsage usage)
        {
            if ((usage & BufferUsage.StagingRead) != 0)
            {
                return BufferUsageHint.StreamRead;
            }
            else if ((usage & BufferUsage.StagingWrite) != 0)
            {
                return BufferUsageHint.StreamCopy;
            }
            else if ((usage & BufferUsage.DynamicRead) != 0)
            {
                return BufferUsageHint.DynamicRead;
            }
            else if ((usage & BufferUsage.DynamicWrite) != 0)
            {
                return BufferUsageHint.DynamicDraw;
            }
            else
            {
                return BufferUsageHint.StaticDraw;
            }
        }

        public void CreateGLResources(IntPtr initialData)
        {
            Debug.Assert(!Created);

            if (_gd.Extensions.ARB_DirectStateAccess)
            {
                uint buffer;
                glCreateBuffers(1, &buffer);
                CheckLastError();
                _buffer = buffer;

                if (_gd.Extensions.ARB_buffer_storage)
                {
                    BufferStorageMask mask = GetStorageMask(Usage);
                    glNamedBufferStorage(
                        _buffer,
                        SizeInBytes,
                        (void*)initialData,
                        mask);
                }
                else
                {
                    BufferUsageHint hint = GetUsageHint(Usage);
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

                glBindBuffer(BufferTarget.CopyWriteBuffer, _buffer);
                CheckLastError();

                if (_gd.Extensions.ARB_buffer_storage)
                {
                    BufferStorageMask mask = GetStorageMask(Usage);
                    glBufferStorage(
                        BufferTarget.CopyWriteBuffer,
                        SizeInBytes,
                        (void*)initialData,
                        mask);
                }
                else
                {
                    BufferUsageHint hint = GetUsageHint(Usage);
                    glBufferData(
                        BufferTarget.CopyWriteBuffer,
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
            glDeleteBuffers(1, &buffer);
            CheckLastError();
            _buffer = buffer;
        }
    }
}
