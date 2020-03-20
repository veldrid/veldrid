using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;

namespace Veldrid.D3D11
{
    internal class D3D11Swapchain : Swapchain
    {
        private readonly SharpDX.Direct3D11.Device _device;
        private readonly PixelFormat? _depthFormat;
        private readonly SwapChain _dxgiSwapChain;
        private bool _vsync;
        private int _syncInterval;
        private D3D11Framebuffer _framebuffer;
        private D3D11Texture _depthTexture;
        private float _pixelScale = 1f;
        private bool _disposed;

        private readonly object _referencedCLsLock = new object();
        private HashSet<D3D11CommandList> _referencedCLs = new HashSet<D3D11CommandList>();

        public override Framebuffer Framebuffer => _framebuffer;

        public override string Name { get => _dxgiSwapChain.DebugName; set => _dxgiSwapChain.DebugName = value; }

        public override bool SyncToVerticalBlank
        {
            get => _vsync; set
            {
                _vsync = value;
                _syncInterval = GetSyncInterval(value);
            }
        }

        private readonly Format _colorFormat;

        public SwapChain DxgiSwapChain => _dxgiSwapChain;

        public int SyncInterval => _syncInterval;

        public D3D11Swapchain(SharpDX.Direct3D11.Device device, ref SwapchainDescription description)
        {
            _device = device;
            _depthFormat = description.DepthFormat;
            SyncToVerticalBlank = description.SyncToVerticalBlank;

            _colorFormat = description.ColorSrgb
                ? Format.B8G8R8A8_UNorm_SRgb
                : Format.B8G8R8A8_UNorm;

            if (description.Source is Win32SwapchainSource win32Source)
            {
                SwapChainDescription dxgiSCDesc = new SwapChainDescription
                {
                    BufferCount = 2,
                    IsWindowed = true,
                    ModeDescription = new ModeDescription(
                        (int)description.Width, (int)description.Height, new Rational(60, 1), _colorFormat),
                    OutputHandle = win32Source.Hwnd,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,
                    Usage = Usage.RenderTargetOutput
                };

                using (SharpDX.DXGI.Device dxgiDevice = _device.QueryInterface<SharpDX.DXGI.Device>())
                {
                    using (Factory dxgiFactory = dxgiDevice.Adapter.GetParent<Factory>())
                    {
                        _dxgiSwapChain = new SwapChain(dxgiFactory, _device, dxgiSCDesc);
                        dxgiFactory.MakeWindowAssociation(win32Source.Hwnd, WindowAssociationFlags.IgnoreAltEnter);
                    }
                }
            }
            else if (description.Source is UwpSwapchainSource uwpSource)
            {
                _pixelScale = uwpSource.LogicalDpi / 96.0f;

                // Properties of the swap chain
                SwapChainDescription1 swapChainDescription = new SwapChainDescription1()
                {
                    AlphaMode = AlphaMode.Ignore,
                    BufferCount = 2,
                    Format = _colorFormat,
                    Height = (int)(description.Height * _pixelScale),
                    Width = (int)(description.Width * _pixelScale),
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.FlipSequential,
                    Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                };

                // Retrive the SharpDX.DXGI device associated to the Direct3D device.
                using (SharpDX.DXGI.Device3 dxgiDevice = _device.QueryInterface<SharpDX.DXGI.Device3>())
                {
                    // Get the SharpDX.DXGI factory automatically created when initializing the Direct3D device.
                    using (Factory2 dxgiFactory = dxgiDevice.Adapter.GetParent<Factory2>())
                    {
                        // Create the swap chain and get the highest version available.
                        using (SwapChain1 swapChain1 = new SwapChain1(dxgiFactory, _device, ref swapChainDescription))
                        {
                            _dxgiSwapChain = swapChain1.QueryInterface<SwapChain2>();
                        }
                    }
                }

                ComObject co = new ComObject(uwpSource.SwapChainPanelNative);

                ISwapChainPanelNative swapchainPanelNative = co.QueryInterfaceOrNull<ISwapChainPanelNative>();
                if (swapchainPanelNative != null)
                {
                    swapchainPanelNative.SwapChain = _dxgiSwapChain;
                }
                else
                {
                    ISwapChainBackgroundPanelNative bgPanelNative = co.QueryInterfaceOrNull<ISwapChainBackgroundPanelNative>();
                    if (bgPanelNative != null)
                    {
                        bgPanelNative.SwapChain = _dxgiSwapChain;
                    }
                }
            }

            Resize(description.Width, description.Height);
        }

        public override void Resize(uint width, uint height)
        {
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
                _dxgiSwapChain.ResizeBuffers(2, (int)actualWidth, (int)actualHeight, _colorFormat, SwapChainFlags.None);
            }

            // Get the backbuffer from the swapchain
            using (Texture2D backBufferTexture = _dxgiSwapChain.GetBackBuffer<Texture2D>(0))
            {
                if (_depthFormat != null)
                {
                    TextureDescription depthDesc = new TextureDescription(
                        actualWidth, actualHeight, 1, 1, 1,
                        _depthFormat.Value,
                        TextureUsage.DepthStencil,
                        TextureType.Texture2D);
                    _depthTexture = new D3D11Texture(_device, ref depthDesc);
                }

                D3D11Texture backBufferVdTexture = new D3D11Texture(
                    backBufferTexture,
                    TextureType.Texture2D,
                    D3D11Formats.ToVdFormat(_colorFormat));
                FramebufferDescription desc = new FramebufferDescription(_depthTexture, backBufferVdTexture);
                _framebuffer = new D3D11Framebuffer(_device, ref desc);
                _framebuffer.Swapchain = this;
            }
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

        private static int GetSyncInterval(bool syncToVBlank)
        {
            return syncToVBlank ? 1 : 0;
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
    }
}
