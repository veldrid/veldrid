using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLSwapchain : Swapchain
    {
        private readonly MTLSwapchainFramebuffer _framebuffer;
        private readonly CAMetalLayer _metalLayer;
        private readonly MTLGraphicsDevice _gd;

        private CAMetalDrawable _drawable;

        public override Framebuffer Framebuffer => _framebuffer;
        public override bool SyncToVerticalBlank { get; set; }
        public override string Name { get; set; }

        public CAMetalDrawable CurrentDrawable => _drawable;

        public MTLSwapchain(MTLGraphicsDevice gd, ref SwapchainDescription description)
        {
            _gd = gd;

            SwapchainSource source = description.Source;
            if (!(source is NSWindowSwapchainSource nsWindowSource))
            {
                throw new VeldridException($"Unsupported Metal SwapchainSource.");
            }

            NSWindow nswindow = new NSWindow(nsWindowSource.NSWindow);
            CGSize windowContentSize = nswindow.contentView.frame.size;
            uint width = (uint)windowContentSize.width;
            uint height = (uint)windowContentSize.height;

            NSView contentView = nswindow.contentView;
            contentView.wantsLayer = true;

            _metalLayer = CAMetalLayer.New();
            contentView.layer = _metalLayer.NativePtr;
            _metalLayer.device = _gd.Device;
            _metalLayer.pixelFormat = MTLPixelFormat.BGRA8Unorm;
            _metalLayer.framebufferOnly = true;
            GetNextDrawable();

            _framebuffer = new MTLSwapchainFramebuffer(
                gd,
                this,
                width,
                height,
                description.DepthFormat,
                PixelFormat.B8_G8_R8_A8_UNorm);
        }

        public void GetNextDrawable()
        {
            if (_drawable.NativePtr != IntPtr.Zero)
            {
                ObjectiveCRuntime.objc_msgSend(_drawable.NativePtr, "release");
            }

            _drawable = _metalLayer.nextDrawable();
        }

        public override void Resize(uint width, uint height)
        {
            _framebuffer.Resize(width, height);
        }

        public override void Dispose()
        {
            if (_drawable.NativePtr != IntPtr.Zero)
            {
                ObjectiveCRuntime.objc_msgSend(_drawable.NativePtr, "release");
            }
            _framebuffer.Dispose();
            ObjectiveCRuntime.release(_metalLayer.NativePtr);
        }
    }
}
