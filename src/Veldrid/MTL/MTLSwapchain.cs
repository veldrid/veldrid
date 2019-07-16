using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLSwapchain : Swapchain
    {
        private readonly MTLSwapchainFramebuffer[] _framebuffers;
        private CAMetalLayer _metalLayer;
        private readonly MTLGraphicsDevice _gd;
        private UIView _uiView; // Valid only when a UIViewSwapchainSource is used.
        private bool _syncToVerticalBlank;
        public uint ImageIndex { get; private set; }

        public override Framebuffer Framebuffer => _framebuffers[ImageIndex];

        public override bool SyncToVerticalBlank
        {
            get => _syncToVerticalBlank;
            set
            {
                if (_syncToVerticalBlank != value)
                {
                    SetSyncToVerticalBlank(value);
                }
            }
        }

        public override string Name { get; set; }

        public override Framebuffer[] Framebuffers => _framebuffers;

        // TODO: Just rename the other property.
        public override uint LastAcquiredImage => ImageIndex;

        public MTLSwapchain(MTLGraphicsDevice gd, ref SwapchainDescription description)
        {
            _gd = gd;
            _syncToVerticalBlank = description.SyncToVerticalBlank;

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
            else if (source is NSViewSwapchainSource nsViewSource)
            {
                NSView contentView = new NSView(nsViewSource.NSView);
                CGSize windowContentSize = contentView.frame.size;
                width = (uint)windowContentSize.width;
                height = (uint)windowContentSize.height;
                contentView.wantsLayer = true;
                contentView.layer = _metalLayer.NativePtr;
            }
            else if (source is UIViewSwapchainSource uiViewSource)
            {
                UIScreen mainScreen = UIScreen.mainScreen;
                CGFloat nativeScale = mainScreen.nativeScale;

                _uiView = new UIView(uiViewSource.UIView);
                CGSize viewSize = _uiView.frame.size;
                width = (uint)(viewSize.width * nativeScale);
                height = (uint)(viewSize.height * nativeScale);
                _metalLayer.frame = _uiView.frame;
                _metalLayer.opaque = true;
                _uiView.layer.addSublayer(_metalLayer.NativePtr);
            }
            else
            {
                throw new VeldridException($"A Metal Swapchain can only be created from an NSWindow, NSView, or UIView.");
            }

            PixelFormat colorFormat = description.ColorSrgb
                ? PixelFormat.B8_G8_R8_A8_UNorm_SRgb
                : PixelFormat.B8_G8_R8_A8_UNorm;

            _metalLayer.device = _gd.Device;
            _metalLayer.pixelFormat = MTLFormats.VdToMTLPixelFormat(colorFormat, false);
            _metalLayer.framebufferOnly = true;
            _metalLayer.drawableSize = new CGSize(width, height);

            _framebuffers = new MTLSwapchainFramebuffer[(int)_metalLayer.maximumDrawableCount];
            for (int i = 0; i < _framebuffers.Length; i++)
            {
                _framebuffers[i] = new MTLSwapchainFramebuffer(
                    gd,
                    this,
                    width, height,
                    description.DepthFormat,
                    colorFormat);
            }

            SetSyncToVerticalBlank(_syncToVerticalBlank);

        }

        public override void Resize(uint width, uint height)
        {
            if (_uiView.NativePtr != IntPtr.Zero)
            {
                UIScreen mainScreen = UIScreen.mainScreen;
                CGFloat nativeScale = mainScreen.nativeScale;
                width = (uint)(width * nativeScale);
                height = (uint)(height * nativeScale);

                _metalLayer.frame = _uiView.frame;
            }

            foreach (MTLSwapchainFramebuffer fb in _framebuffers)
            {
                fb.Resize(width, height);
            }
            _metalLayer.drawableSize = new CGSize(width, height);
            if (_uiView.NativePtr != IntPtr.Zero)
            {
                _metalLayer.frame = _uiView.frame;
            }

            ImageIndex = (uint)_framebuffers.Length - 1;
            AcquireNextImage();
        }

        private void SetSyncToVerticalBlank(bool value)
        {
            _syncToVerticalBlank = value;

            if (_gd.MetalFeatures.MaxFeatureSet == MTLFeatureSet.macOS_GPUFamily1_v3
                || _gd.MetalFeatures.MaxFeatureSet == MTLFeatureSet.macOS_GPUFamily1_v4
                || _gd.MetalFeatures.MaxFeatureSet == MTLFeatureSet.macOS_GPUFamily2_v1)
            {
                _metalLayer.displaySyncEnabled = value;
            }
        }

        public override void Dispose()
        {
            foreach (MTLSwapchainFramebuffer fb in _framebuffers)
            {
                fb.Dispose();
            }
            ObjectiveCRuntime.release(_metalLayer.NativePtr);
        }

        internal uint AcquireNextImage()
        {
            ImageIndex = (ImageIndex + 1) % BufferCount;
            using (NSAutoreleasePool.Begin())
            {
                CAMetalDrawable drawable = _metalLayer.nextDrawable();
                ObjectiveCRuntime.retain(drawable.NativePtr);
                _framebuffers[ImageIndex].SetDrawable(drawable);
            }

            return ImageIndex;
        }
    }
}
