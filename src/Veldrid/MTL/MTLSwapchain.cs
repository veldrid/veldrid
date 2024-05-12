﻿using System;
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
        private bool _syncToVerticalBlank;
        private bool _disposed;

        private CAMetalDrawable _drawable;

        public override Framebuffer Framebuffer => _framebuffer;
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

        public override bool IsDisposed => _disposed;

        public CAMetalDrawable CurrentDrawable => _drawable;

        public MTLSwapchain(MTLGraphicsDevice gd, ref SwapchainDescription description)
        {
            _gd = gd;
            _syncToVerticalBlank = description.SyncToVerticalBlank;

            uint width;
            uint height;

            SwapchainSource source = description.Source;
            if (source is NSWindowSwapchainSource nsWindowSource)
            {
                NSWindow nswindow = new NSWindow(nsWindowSource.NSWindow);
                NSView contentView = nswindow.contentView;
                CGSize windowContentSize = contentView.frame.size;
                width = (uint)windowContentSize.width;
                height = (uint)windowContentSize.height;

                if (!CAMetalLayer.TryCast(contentView.layer, out _metalLayer))
                {
                    _metalLayer = CAMetalLayer.New();
                    contentView.wantsLayer = true;
                    contentView.layer = _metalLayer.NativePtr;
                }
            }
            else if (source is NSViewSwapchainSource nsViewSource)
            {
                NSView contentView = new NSView(nsViewSource.NSView);
                CGSize windowContentSize = contentView.frame.size;
                width = (uint)windowContentSize.width;
                height = (uint)windowContentSize.height;

                if (!CAMetalLayer.TryCast(contentView.layer, out _metalLayer))
                {
                    _metalLayer = CAMetalLayer.New();
                    contentView.wantsLayer = true;
                    contentView.layer = _metalLayer.NativePtr;
                }
            }
            else if (source is UIViewSwapchainSource uiViewSource)
            {
                UIScreen mainScreen = UIScreen.mainScreen;
                CGFloat nativeScale = mainScreen.nativeScale;

                _uiView = new UIView(uiViewSource.UIView);
                CGSize viewSize = _uiView.frame.size;
                width = (uint)(viewSize.width * nativeScale);
                height = (uint)(viewSize.height * nativeScale);

                if (!CAMetalLayer.TryCast(_uiView.layer, out _metalLayer))
                {
                    _metalLayer = CAMetalLayer.New();
                    _metalLayer.frame = _uiView.frame;
                    _metalLayer.opaque = true;
                    _uiView.layer.addSublayer(_metalLayer.NativePtr);
                }
            }
            else
            {
                throw new VeldridException($"A Metal Swapchain can only be created from an NSWindow, NSView, or UIView.");
            }

            PixelFormat format = description.ColorSrgb
                ? PixelFormat.B8_G8_R8_A8_UNorm_SRgb
                : PixelFormat.B8_G8_R8_A8_UNorm;

            _metalLayer.device = _gd.Device;
            _metalLayer.pixelFormat = MTLFormats.VdToMTLPixelFormat(format, false);
            _metalLayer.framebufferOnly = true;
            _metalLayer.drawableSize = new CGSize(width, height);

            SetSyncToVerticalBlank(_syncToVerticalBlank);

            _framebuffer = new MTLSwapchainFramebuffer(
                gd,
                this,
                description.DepthFormat,
                format);

            getNextDrawable();
        }

        private bool getNextDrawable()
        {
            if (!_drawable.IsNull)
            {
                ObjectiveCRuntime.release(_drawable.NativePtr);
            }

            using (NSAutoreleasePool.Begin())
            {
                _drawable = _metalLayer.nextDrawable();

                if (!_drawable.IsNull)
                {
                    ObjectiveCRuntime.retain(_drawable.NativePtr);
                    _framebuffer.UpdateTextures(_drawable, _metalLayer.drawableSize);
                    return true;
                }

                return false;
            }
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

            _metalLayer.drawableSize = new CGSize(width, height);
            if (_uiView.NativePtr != IntPtr.Zero)
            {
                _metalLayer.frame = _uiView.frame;
            }

            getNextDrawable();
        }

        public bool EnsureDrawableAvailable() => !_drawable.IsNull || getNextDrawable();

        public void InvalidateDrawable()
        {
            ObjectiveCRuntime.release(_drawable.NativePtr);
            _drawable = default;
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
            if (_drawable.NativePtr != IntPtr.Zero)
            {
                ObjectiveCRuntime.objc_msgSend(_drawable.NativePtr, "release");
            }
            _framebuffer.Dispose();
            ObjectiveCRuntime.release(_metalLayer.NativePtr);

            _disposed = true;
        }
    }
}
