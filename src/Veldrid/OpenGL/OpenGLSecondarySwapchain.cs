using System;
using System.Runtime.InteropServices;

namespace Veldrid.OpenGL
{
    internal class OpenGLSecondarySwapchain : Swapchain, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly OpenGLSubordinateContext _subordinateContext;
        private readonly SwapchainSource _swapchainSource;
        private readonly Framebuffer[] _framebuffers;
        private bool _syncToVerticalBlank;
        private OpenGLSwapchainFramebuffer _fb;
        private bool _disposed;

        public OpenGLSecondarySwapchain(OpenGLGraphicsDevice gd, ref SwapchainDescription scDesc)
        {
            _gd = gd;
            _swapchainSource = scDesc.Source;
            OpenGLPlatformInfo info = gd.CreateSharedContext(
                new GraphicsDeviceOptions(),
                scDesc,
                _gd.BackendType,
                _gd.ContextHandle);
            bool backbufferIsSrgb = scDesc.ColorSrgb;
            info.ClearCurrentContext();
            _syncToVerticalBlank = scDesc.SyncToVerticalBlank;


            PixelFormat swapchainFormat;
            if (scDesc.ColorSrgb && (backbufferIsSrgb || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
            {
                swapchainFormat = PixelFormat.B8_G8_R8_A8_UNorm_SRgb;
            }
            else
            {
                swapchainFormat = PixelFormat.B8_G8_R8_A8_UNorm;
            }

            _swapchainSource.GetSize(out uint width, out uint height);

            _fb = new OpenGLSwapchainFramebuffer(
                _gd,
                width, height,
                swapchainFormat,
                scDesc.DepthFormat,
                false,
                true);
            _subordinateContext = new OpenGLSubordinateContext(info, _fb);
            _subordinateContext.Resize(width, height);

            _framebuffers = new[] { _fb, _fb };

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

        public override Framebuffer[] Framebuffers => _framebuffers;

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

        public override void Resize()
        {
            _swapchainSource.GetSize(out uint width, out uint height);
            Resize(width, height);
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
