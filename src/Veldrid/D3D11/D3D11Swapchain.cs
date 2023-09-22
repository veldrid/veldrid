﻿using Vortice;
using Vortice.Direct3D11;
using Vortice.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using SharpGen.Runtime;

namespace Veldrid.D3D11
{
    internal class D3D11Swapchain : Swapchain
    {
        private readonly D3D11GraphicsDevice _gd;
        private readonly SwapchainDescription _description;
        private readonly PixelFormat? _depthFormat;
        private IDXGISwapChain _dxgiSwapChain;
        private bool _vsync;
        private int _syncInterval;
        private D3D11Framebuffer _framebuffer;
        private D3D11Texture _depthTexture;
        private float _pixelScale = 1f;
        private SwapChainFlags _flags;
        private bool _disposed;
        private FrameLatencyWaitHandle _frameLatencyWaitHandle;

        private readonly object _referencedCLsLock = new object();
        private HashSet<D3D11CommandList> _referencedCLs = new HashSet<D3D11CommandList>();

        public override Framebuffer Framebuffer => _framebuffer;

        public override string Name
        {
            get
            {
                unsafe
                {
                    byte* pname = stackalloc byte[1024];
                    int size = 1024 - 1;
                    _dxgiSwapChain.GetPrivateData(CommonGuid.DebugObjectName, ref size, new IntPtr(pname));
                    pname[size] = 0;
                    return Marshal.PtrToStringAnsi(new IntPtr(pname));
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _dxgiSwapChain.SetPrivateData(CommonGuid.DebugObjectName, 0, IntPtr.Zero);
                }
                else
                {
                    var namePtr = Marshal.StringToHGlobalAnsi(value);
                    _dxgiSwapChain.SetPrivateData(CommonGuid.DebugObjectName, value.Length, namePtr);
                    Marshal.FreeHGlobal(namePtr);
                }
            }
        }

        public override bool SyncToVerticalBlank
        {
            get => _vsync;
            set
            {
                _vsync = value;
                _syncInterval = D3D11Util.GetSyncInterval(value);
            }
        }

        public PresentFlags PresentFlags
        {
            get
            {
                if (AllowTearing && _canTear && !SyncToVerticalBlank)
                    return PresentFlags.AllowTearing;

                return PresentFlags.None;
            }
        }

        private bool _allowTearing;

        public bool AllowTearing
        {
            get => _allowTearing;
            set
            {
                if (_allowTearing == value)
                    return;

                _allowTearing = value;

                if (!_canTear)
                    return;

                recreateSwapchain();
            }
        }

        private uint _width;
        private uint _height;

        private readonly bool _canTear;
        private readonly bool _canCreateFrameLatencyWaitableObject;
        private readonly Format _colorFormat;

        public IDXGISwapChain DxgiSwapChain => _dxgiSwapChain;

        public int SyncInterval => _syncInterval;

        public D3D11Swapchain(D3D11GraphicsDevice gd, ref SwapchainDescription description)
        {
            _gd = gd;
            _description = description;
            _depthFormat = description.DepthFormat;
            SyncToVerticalBlank = description.SyncToVerticalBlank;

            _colorFormat = description.ColorSrgb
                ? Format.B8G8R8A8_UNorm_SRgb
                : Format.B8G8R8A8_UNorm;

            using (IDXGIFactory5 dxgiFactory5 = _gd.Adapter.GetParent<IDXGIFactory5>())
                _canTear = dxgiFactory5?.PresentAllowTearing == true;

            using (IDXGIFactory3 dxgiFactory3 = _gd.Adapter.GetParent<IDXGIFactory3>())
                _canCreateFrameLatencyWaitableObject = dxgiFactory3 != null;

            _width = description.Width;
            _height = description.Height;

            recreateSwapchain();
        }

        private void recreateSwapchain()
        {
            _dxgiSwapChain?.Release();
            _dxgiSwapChain?.Dispose();
            _dxgiSwapChain = null;

            _framebuffer?.Dispose();
            _framebuffer = null;

            _depthTexture?.Dispose();
            _depthTexture = null;

            _frameLatencyWaitHandle?.Dispose();
            _frameLatencyWaitHandle = null;

            _flags = SwapChainFlags.None;

            if (AllowTearing && _canTear)
                _flags |= SwapChainFlags.AllowTearing;
            else if (_canCreateFrameLatencyWaitableObject)
                _flags |= SwapChainFlags.FrameLatencyWaitableObject;

            if (_description.Source is Win32SwapchainSource win32Source)
            {
                SwapChainDescription dxgiSCDesc = new SwapChainDescription
                {
                    BufferCount = 2,
                    Windowed = true,
                    BufferDescription = new ModeDescription(
                        (int)_width, (int)_height, _colorFormat),
                    OutputWindow = win32Source.Hwnd,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,
                    BufferUsage = Usage.RenderTargetOutput,
                    Flags = _flags
                };

                using (IDXGIFactory dxgiFactory = _gd.Adapter.GetParent<IDXGIFactory>())
                {
                    _dxgiSwapChain = dxgiFactory.CreateSwapChain(_gd.Device, dxgiSCDesc);
                    dxgiFactory.MakeWindowAssociation(win32Source.Hwnd, WindowAssociationFlags.IgnoreAltEnter);
                }
            }
            else if (_description.Source is UwpSwapchainSource uwpSource)
            {
                _pixelScale = uwpSource.LogicalDpi / 96.0f;

                // Properties of the swap chain
                SwapChainDescription1 swapChainDescription = new SwapChainDescription1()
                {
                    AlphaMode = AlphaMode.Ignore,
                    BufferCount = 2,
                    Format = _colorFormat,
                    Height = (int)(_height * _pixelScale),
                    Width = (int)(_width * _pixelScale),
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.FlipSequential,
                    BufferUsage = Usage.RenderTargetOutput,
                    Flags = _flags
                };

                // Get the Vortice.DXGI factory automatically created when initializing the Direct3D device.
                using (IDXGIFactory2 dxgiFactory = _gd.Adapter.GetParent<IDXGIFactory2>())
                {
                    // Create the swap chain and get the highest version available.
                    using (IDXGISwapChain1 swapChain1 = dxgiFactory.CreateSwapChainForComposition(_gd.Device, swapChainDescription))
                    {
                        _dxgiSwapChain = swapChain1.QueryInterface<IDXGISwapChain2>();
                    }
                }

                ComObject co = new ComObject(uwpSource.SwapChainPanelNative);

                ISwapChainPanelNative swapchainPanelNative = co.QueryInterfaceOrNull<ISwapChainPanelNative>();
                if (swapchainPanelNative != null)
                {
                    swapchainPanelNative.SetSwapChain(_dxgiSwapChain);
                }
                else
                {
                    ISwapChainBackgroundPanelNative bgPanelNative = co.QueryInterfaceOrNull<ISwapChainBackgroundPanelNative>();
                    if (bgPanelNative != null)
                    {
                        bgPanelNative.SetSwapChain(_dxgiSwapChain);
                    }
                }
            }

            if ((_flags & SwapChainFlags.FrameLatencyWaitableObject) > 0)
            {
                using (IDXGISwapChain2 swapChain2 = _dxgiSwapChain.QueryInterfaceOrNull<IDXGISwapChain2>())
                {
                    if (swapChain2 != null)
                    {
                        swapChain2.MaximumFrameLatency = 1;
                        _frameLatencyWaitHandle = new FrameLatencyWaitHandle(swapChain2.FrameLatencyWaitableObject);
                    }
                }
            }

            Resize(_width, _height);
        }

        public override void Resize(uint width, uint height)
        {
            _width = width;
            _height = height;

            lock (_referencedCLsLock)
            {
                foreach (D3D11CommandList cl in _referencedCLs)
                {
                    cl.Reset();
                }

                _referencedCLs.Clear();
            }

            bool resizeBuffers = false;

            if (_framebuffer != null)
            {
                resizeBuffers = true;
                if (_depthTexture != null)
                {
                    _depthTexture.Dispose();
                }

                _framebuffer.Dispose();
            }

            uint actualWidth = (uint)(width * _pixelScale);
            uint actualHeight = (uint)(height * _pixelScale);
            if (resizeBuffers)
            {
                _dxgiSwapChain.ResizeBuffers(2, (int)actualWidth, (int)actualHeight, _colorFormat, _flags).CheckError();
            }

            // Get the backbuffer from the swapchain
            using (ID3D11Texture2D backBufferTexture = _dxgiSwapChain.GetBuffer<ID3D11Texture2D>(0))
            {
                if (_depthFormat != null)
                {
                    TextureDescription depthDesc = new TextureDescription(
                        actualWidth, actualHeight, 1, 1, 1,
                        _depthFormat.Value,
                        TextureUsage.DepthStencil,
                        TextureType.Texture2D);
                    _depthTexture = new D3D11Texture(_gd.Device, ref depthDesc);
                }

                D3D11Texture backBufferVdTexture = new D3D11Texture(
                    backBufferTexture,
                    TextureType.Texture2D,
                    D3D11Formats.ToVdFormat(_colorFormat));

                FramebufferDescription desc = new FramebufferDescription(_depthTexture, backBufferVdTexture);
                _framebuffer = new D3D11Framebuffer(_gd.Device, ref desc)
                {
                    Swapchain = this
                };
            }
        }

        public void WaitForNextFrameReady()
        {
            _frameLatencyWaitHandle?.WaitOne(1000);
        }

        public void AddCommandListReference(D3D11CommandList cl)
        {
            lock (_referencedCLsLock)
            {
                _referencedCLs.Add(cl);
            }
        }

        public void RemoveCommandListReference(D3D11CommandList cl)
        {
            lock (_referencedCLsLock)
            {
                _referencedCLs.Remove(cl);
            }
        }

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            if (!_disposed)
            {
                _depthTexture?.Dispose();
                _framebuffer.Dispose();
                _dxgiSwapChain.Dispose();

                _disposed = true;
            }
        }

        private class FrameLatencyWaitHandle : WaitHandle
        {
            public FrameLatencyWaitHandle(IntPtr ptr)
            {
                SafeWaitHandle = new SafeWaitHandle(ptr, true);
            }
        }
    }
}
