using static Vd2.OpenGLBinding.OpenGLNative;
using static Vd2.OpenGL.OpenGLUtil;
using System;
using Vd2.OpenGLBinding;
using System.Collections.Concurrent;

namespace Vd2.OpenGL
{
    internal unsafe class OpenGLGraphicsDevice : GraphicsDevice
    {
        private readonly uint _vao;
        private readonly ConcurrentQueue<OpenGLDeferredResource> _resourcesToDispose
            = new ConcurrentQueue<OpenGLDeferredResource>();
        private readonly Action _swapBuffers;
        private readonly OpenGLSwapchainFramebuffer _swapchainFramebuffer;

        public override GraphicsBackend BackendType => GraphicsBackend.OpenGL;

        public override ResourceFactory ResourceFactory { get; }

        public override Framebuffer SwapchainFramebuffer => _swapchainFramebuffer;

        public OpenGLGraphicsDevice(
            IntPtr glContext,
            Func<string, IntPtr> getProcAddress,
            Action swapBuffers,
            uint width,
            uint height,
            bool debugDevice)
        {
            _swapBuffers = swapBuffers;
            LoadAllFunctions(glContext, getProcAddress);

            ResourceFactory = new OpenGLResourceFactory(this);

            glGenVertexArrays(1, out _vao);
            CheckLastError();

            glBindVertexArray(_vao);
            CheckLastError();

            _swapchainFramebuffer = new OpenGLSwapchainFramebuffer(width, height);

            PostContextCreated();
        }

        public override void ExecuteCommands(CommandList cl)
        {
            throw new System.NotImplementedException();
        }

        public override void ResizeMainWindow(uint width, uint height)
        {
            _swapchainFramebuffer.Resize(width, height);
        }

        public override void SwapBuffers()
        {
            _swapBuffers();
        }

        public override void WaitForIdle()
        {
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

        /*
        public void EnableDebugCallback() => EnableDebugCallback(DebugSeverity.DebugSeverityNotification);
        public void EnableDebugCallback(DebugSeverity minimumSeverity) => EnableDebugCallback(DefaultDebugCallback(minimumSeverity));
        public void EnableDebugCallback(DebugProc callback)
        {
            GL.Enable(EnableCap.DebugOutput);
            // The debug callback delegate must be persisted, otherwise errors will occur
            // when the OpenGL drivers attempt to call it after it has been collected.
            _debugMessageCallback = callback;
            GL.DebugMessageCallback(_debugMessageCallback, IntPtr.Zero);
        }

        private DebugProc DefaultDebugCallback(DebugSeverity minimumSeverity)
        {
            return (DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam) =>
            {
                if (severity >= minimumSeverity)
                {
                    string messageString = Marshal.PtrToStringAnsi(message, length);
                    System.Diagnostics.Debug.WriteLine($"GL DEBUG MESSAGE: {source}, {type}, {id}. {severity}: {messageString}");
                }
            };
        }
        */

        public override void Dispose()
        {
        }
    }
}
