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

        private readonly Action<IntPtr> _makeCurrent;
        private int _contextCurrentThreadID;

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

            PostContextCreated();
        }

        public override void ExecuteCommands(CommandList cl)
        {
            EnsureCurrentContext();
            OpenGLCommandList glCommandList = Util.AssertSubtype<CommandList, OpenGLCommandList>(cl);
            glCommandList.Commands.ExecuteAll(_commandExecutor);
            glCommandList.Reset();
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
                case OpenGLTexture2D tex2D:
                    tex2D.Name = name;
                    break;
                case OpenGLTextureCube texCube:
                    texCube.Name = name;
                    break;
            }
        }

        public override void WaitForIdle()
        {
            FlushDisposables();
        }

        internal void EnqueueDisposal(OpenGLDeferredResource resource)
        {
            _resourcesToDispose.Enqueue(resource);
        }

        private void FlushDisposables()
        {
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
            // The debug callback delegate must be persisted, otherwise errors will occur
            // when the OpenGL drivers attempt to call it after it has been collected.
            _debugMessageCallback = callback;
            glDebugMessageCallback(_debugMessageCallback, null);
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
            FlushDisposables();
            _deleteContext(_glContext);
        }
    }
}
