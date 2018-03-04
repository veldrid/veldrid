using System;
using System.Diagnostics;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLSwapchain : Swapchain
    {
        private readonly MTLSwapchainFramebuffer _framebuffer;
        private CAMetalLayer _metalLayer;
        private readonly MTLGraphicsDevice _gd;
        private UIView _uiView; // Valid only when a UIViewSwapchainSource is used.

        private CAMetalDrawable _drawable;

        public override Framebuffer Framebuffer => _framebuffer;
        public override bool SyncToVerticalBlank { get; set; }
        public override string Name { get; set; }

        public CAMetalDrawable CurrentDrawable => _drawable;

        public MTLSwapchain(MTLGraphicsDevice gd, ref SwapchainDescription description)
        {
            _gd = gd;
            SyncToVerticalBlank = description.SyncToVerticalBlank;

            _metalLayer = CAMetalLayer.New();

            uint width;
            uint height;

            SwapchainSource source = description.Source;
            if (source is NSWindowSwapchainSource nsWindowSource)
            {
                NSWindow nswindow = new NSWindow(nsWindowSource.NSWindow);
                CGSize windowContentSize = nswindow.contentView.frame.size;
                width = (uint)windowContentSize.width;
                height = (uint)windowContentSize.height;
                NSView contentView = nswindow.contentView;
                contentView.wantsLayer = true;
                contentView.layer = _metalLayer.NativePtr;
            }
            else if (source is UIViewSwapchainSource uiViewSource)
            {
                _uiView = new UIView(uiViewSource.UIView);
                CGSize viewSize = _uiView.frame.size;
                width = (uint)viewSize.width;
                height = (uint)viewSize.height;
                _metalLayer.frame = _uiView.frame;
                _metalLayer.opaque = true;
                _uiView.layer.addSublayer(_metalLayer.NativePtr);
            }
            else
            {
                throw new VeldridException($"A Metal Swapchain can only be created from an NSWindow or UIView.");
            }

            _metalLayer.device = _gd.Device;
            _metalLayer.pixelFormat = MTLPixelFormat.BGRA8Unorm;
            _metalLayer.framebufferOnly = true;
            _metalLayer.drawableSize = new CGSize(width, height);
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
            if (!_drawable.IsNull)
            {
                ObjectiveCRuntime.release(_drawable.NativePtr);
            }

            using (NSAutoreleasePool.Begin())
            {
                _drawable = _metalLayer.nextDrawable();
                ObjectiveCRuntime.retain(_drawable.NativePtr);
            }
        }

        public override void Resize(uint width, uint height)
        {
            _framebuffer.Resize(width, height);
            _metalLayer.drawableSize = new CGSize(width, height);
            if (_uiView.NativePtr != IntPtr.Zero)
            {
                _metalLayer.frame = _uiView.frame;
            }
            GetNextDrawable();
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
