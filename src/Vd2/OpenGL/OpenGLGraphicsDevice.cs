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

        public override GraphicsBackend BackendType => GraphicsBackend.OpenGL;

        public override ResourceFactory ResourceFactory { get; }

        public override Framebuffer SwapchainFramebuffer { get; }

        public OpenGLGraphicsDevice(
            IntPtr glContext,
            Func<string, IntPtr> getProcAddress,
            Action swapBuffer,
            uint width,
            uint height)
        {
            LoadAllFunctions(glContext, getProcAddress);

            ResourceFactory = new OpenGLResourceFactory(this);

            glGenVertexArrays(1, out _vao);
            CheckLastError();

            glBindVertexArray(_vao);
            CheckLastError();

            PostContextCreated();
        }

        public override void ExecuteCommands(CommandList cl)
        {
            throw new System.NotImplementedException();
        }

        public override void ResizeMainWindow(uint width, uint height)
        {
            throw new System.NotImplementedException();
        }

        public override void SwapBuffers()
        {
            throw new System.NotImplementedException();
        }

        public override void WaitForIdle()
        {
            throw new System.NotImplementedException();
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

        public override void Dispose()
        {
        }
    }
}
