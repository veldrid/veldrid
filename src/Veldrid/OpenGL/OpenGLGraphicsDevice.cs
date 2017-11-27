using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using System;
using Veldrid.OpenGLBinding;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLGraphicsDevice : GraphicsDevice
    {
        private readonly uint _vao;
        private readonly ConcurrentQueue<OpenGLDeferredResource> _resourcesToDispose
            = new ConcurrentQueue<OpenGLDeferredResource>();
        private readonly IntPtr _glContext;
        private readonly Action<IntPtr> _deleteContext;
        private readonly Action _swapBuffers;
        private readonly OpenGLSwapchainFramebuffer _swapchainFramebuffer;
        private readonly OpenGLCommandExecutor _commandExecutor;
        private DebugProc _debugMessageCallback;
        private readonly OpenGLExtensions _extensions;
        private readonly object _contextLock = new object();

        private readonly Action<IntPtr> _makeCurrent;
        private int _contextCurrentThreadID;
        private readonly TextureSampleCount _maxColorTextureSamples;

        public override GraphicsBackend BackendType => GraphicsBackend.OpenGL;

        public override ResourceFactory ResourceFactory { get; }

        public override Framebuffer SwapchainFramebuffer => _swapchainFramebuffer;

        public OpenGLExtensions Extensions => _extensions;

        public OpenGLGraphicsDevice(
            OpenGLPlatformInfo platformInfo,
            uint width,
            uint height,
            bool debugDevice)
        {
            _glContext = platformInfo.OpenGLContextHandle;
            _makeCurrent = platformInfo.MakeCurrent;
            _deleteContext = platformInfo.DeleteContext;
            _swapBuffers = platformInfo.SwapBuffers;
            LoadAllFunctions(_glContext, platformInfo.GetProcAddress);

            ResourceFactory = new OpenGLResourceFactory(this);

            glGenVertexArrays(1, out _vao);
            CheckLastError();

            glBindVertexArray(_vao);
            CheckLastError();

            _swapchainFramebuffer = new OpenGLSwapchainFramebuffer(width, height);

            if (debugDevice)
            {
                EnableDebugCallback();
            }

            // Set miscellaneous initial states.
            glEnable(EnableCap.TextureCubeMapSeamless);
            CheckLastError();

            int extensionCount;
            glGetIntegerv(GetPName.NumExtensions, &extensionCount);
            CheckLastError();

            HashSet<string> extensions = new HashSet<string>();
            for (uint i = 0; i < extensionCount; i++)
            {
                byte* extensionNamePtr = glGetStringi(StringNameIndexed.Extensions, i);
                CheckLastError();
                if (extensionNamePtr != null)
                {
                    string extensionName = Util.GetString(extensionNamePtr);
                    extensions.Add(extensionName);
                }
            }

            _extensions = new OpenGLExtensions(extensions);
            _commandExecutor = new OpenGLCommandExecutor(_extensions);

            int maxColorTextureSamples;
            glGetIntegerv(GetPName.MaxColorTextureSamples, &maxColorTextureSamples);
            CheckLastError();
            if (maxColorTextureSamples >= 32)
            {
                _maxColorTextureSamples = TextureSampleCount.Count32;
            }
            else if (maxColorTextureSamples >= 16)
            {
                _maxColorTextureSamples = TextureSampleCount.Count16;
            }
            else if (maxColorTextureSamples >= 8)
            {
                _maxColorTextureSamples = TextureSampleCount.Count8;
            }
            else if (maxColorTextureSamples >= 4)
            {
                _maxColorTextureSamples = TextureSampleCount.Count4;
            }
            else if (maxColorTextureSamples >= 2)
            {
                _maxColorTextureSamples = TextureSampleCount.Count2;
            }
            else
            {
                _maxColorTextureSamples = TextureSampleCount.Count1;
            }

            PostDeviceCreated();
        }

        public override void ExecuteCommands(CommandList cl)
        {
            lock (_contextLock)
            {
                EnsureCurrentContext();
                OpenGLCommandList glCommandList = Util.AssertSubtype<CommandList, OpenGLCommandList>(cl);
                glCommandList.Commands.ExecuteAll(_commandExecutor);
                glCommandList.Reset();
            }
        }

        private void EnsureCurrentContext()
        {
            int currentThreadID = Environment.CurrentManagedThreadId;
            if (_contextCurrentThreadID != currentThreadID)
            {
                _contextCurrentThreadID = currentThreadID;
                _makeCurrent(_glContext);
            }
        }

        public override void ResizeMainWindow(uint width, uint height)
        {
            _swapchainFramebuffer.Resize(width, height);
        }

        public override void SwapBuffers()
        {
            _swapBuffers();
            FlushDisposables();
        }

        public override void SetResourceName(DeviceResource resource, string name)
        {
            switch (resource)
            {
                case OpenGLBuffer buffer:
                    buffer.Name = name;
                    break;
                case OpenGLFramebuffer framebuffer:
                    framebuffer.Name = name;
                    break;
                case OpenGLSampler sampler:
                    sampler.Name = name;
                    break;
                case OpenGLShader shader:
                    shader.Name = name;
                    break;
                case OpenGLTexture tex:
                    tex.Name = name;
                    break;
            }
        }

        public override void WaitForIdle()
        {
            FlushDisposables();
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            return _maxColorTextureSamples;
        }

        protected override MappedResource MapCore(MappableResource resource, uint subresource)
        {
            if (resource is OpenGLBuffer buffer)
            {
                void* mappedPtr;
                lock (_contextLock)
                {
                    buffer.EnsureResourcesCreated();
                    glBindBuffer(BufferTarget.CopyReadBuffer, buffer.Buffer);
                    CheckLastError();

                    mappedPtr = glMapBuffer(BufferTarget.CopyReadBuffer, BufferAccess.ReadWrite);
                    CheckLastError();
                }

                return new MappedResource(resource, (IntPtr)mappedPtr, buffer.SizeInBytes);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected override void UnmapCore(MappableResource resource, uint subresource)
        {
            if (resource is OpenGLBuffer buffer)
            {
                lock (_contextLock)
                {
                    glBindBuffer(BufferTarget.CopyReadBuffer, buffer.Buffer);
                    CheckLastError();

                    glUnmapBuffer(BufferTarget.CopyReadBuffer);
                    CheckLastError();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            lock (_contextLock)
            {
                _commandExecutor.UpdateBuffer(buffer, bufferOffsetInBytes, source, sizeInBytes);
            }
        }

        public override void UpdateTexture(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            lock (_contextLock)
            {
                _commandExecutor.UpdateTexture(texture, source, x, y, z, width, height, depth, mipLevel, arrayLayer);
            }
        }

        public override void UpdateTextureCube(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            lock (_contextLock)
            {
                _commandExecutor.UpdateTextureCube(texture, source, face, x, y, width, height, mipLevel, arrayLayer);
            }
        }

        internal void EnqueueDisposal(OpenGLDeferredResource resource)
        {
            _resourcesToDispose.Enqueue(resource);
        }

        private void FlushDisposables()
        {
            // Check if the OpenGL context has already been destroyed by the OS. If so, just exit out.
            uint error = glGetError();
            if (error == (uint)ErrorCode.InvalidOperation)
            {
                return;
            }

            while (_resourcesToDispose.TryDequeue(out OpenGLDeferredResource resource))
            {
                resource.DestroyGLResources();
            }
        }

        public void EnableDebugCallback() => EnableDebugCallback(DebugSeverity.DebugSeverityNotification);
        public void EnableDebugCallback(DebugSeverity minimumSeverity) => EnableDebugCallback(DefaultDebugCallback(minimumSeverity));
        public void EnableDebugCallback(DebugProc callback)
        {
            glEnable(EnableCap.DebugOutput);
            CheckLastError();
            // The debug callback delegate must be persisted, otherwise errors will occur
            // when the OpenGL drivers attempt to call it after it has been collected.
            _debugMessageCallback = callback;
            glDebugMessageCallback(_debugMessageCallback, null);
            CheckLastError();
        }

        private DebugProc DefaultDebugCallback(DebugSeverity minimumSeverity)
        {
            return (source, type, id, severity, length, message, userParam) =>
            {
                if (severity >= minimumSeverity)
                {
                    string messageString = Marshal.PtrToStringAnsi((IntPtr)message, (int)length);
                    System.Diagnostics.Debug.WriteLine($"GL DEBUG MESSAGE: {source}, {type}, {id}. {severity}: {messageString}");
                }
            };
        }

        protected override void PlatformDispose()
        {
            EnsureCurrentContext();
            FlushDisposables();
            _deleteContext(_glContext);
        }
    }
}
