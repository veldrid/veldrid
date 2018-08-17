using System;

namespace Veldrid.OpenGL
{
    internal class OpenGLSecondarySwapchain : Swapchain, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly OpenGLSubordinateContext _subordinateContext;
        private bool _syncToVerticalBlank;
        private OpenGLSwapchainFramebuffer _fb;
        private bool _disposed;

        public OpenGLSecondarySwapchain(OpenGLGraphicsDevice gd, ref SwapchainDescription scDesc)
        {
            _gd = gd;
            OpenGLPlatformInfo info = gd.CreateSharedContext(
                new GraphicsDeviceOptions(),
                scDesc,
                _gd.BackendType,
                _gd.ContextHandle);
            info.ClearCurrentContext();
            _syncToVerticalBlank = scDesc.SyncToVerticalBlank;
            _fb = new OpenGLSwapchainFramebuffer(
                _gd,
                scDesc.Width, scDesc.Height,
                PixelFormat.B8_G8_R8_A8_UNorm,
                scDesc.DepthFormat,
                true);
            _subordinateContext = new OpenGLSubordinateContext(info, _fb);
            _subordinateContext.Resize(scDesc.Width, scDesc.Height);

            _gd.RegisterSecondarySwapchain(this);
        }

        public override Framebuffer Framebuffer => _fb;

        public override bool SyncToVerticalBlank
        {
            get => _syncToVerticalBlank;
            set => _subordinateContext.SetSyncToVerticalBlank(value);
        }

        public override string Name { get; set; }

        public bool Created { get; private set; }

        public void DestroyGLResources()
        {
            if (!_disposed)
            {
                _disposed = true;
                DestroyFramebuffer();
            }
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
        }

        public override void Resize(uint width, uint height)
        {
            _fb.Resize(width, height);
            _subordinateContext.Resize(width, height);
        }

        public void SwapBuffers(IntPtr sync)
        {
            _subordinateContext.SwapBuffers(sync);
        }

        public void EnsureResourcesCreated()
        {
        }

        private void DestroyFramebuffer()
        {
            _subordinateContext.Terminate();
            _fb.Dispose();
        }
    }
}
