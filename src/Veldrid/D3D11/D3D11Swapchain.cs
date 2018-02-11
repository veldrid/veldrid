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
        private int _pixelScale = 1;

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

        public SwapChain DxgiSwapChain => _dxgiSwapChain;

        public int SyncInterval => _syncInterval;

        public D3D11Swapchain(SharpDX.Direct3D11.Device device, ref SwapchainDescription description)
        {
            _device = device;
            _depthFormat = description.DepthFormat;
            if (description.Source is Win32SwapchainSource win32Source)
            {
                SwapChainDescription dxgiSCDesc = new SwapChainDescription
                {
                    BufferCount = 2,
                    IsWindowed = true,
                    ModeDescription = new ModeDescription(
                        (int)description.Width, (int)description.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
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
                    }
                }
            }
            else
            {
                // TODO-SWAPCHAIN: Implement UWP swapchains.
                throw new NotImplementedException();
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

            if (_framebuffer != null)
            {
                if (_depthTexture != null)
                {
                    _depthTexture.Dispose();
                }

                _framebuffer.Dispose();
            }

            uint actualWidth = (uint)(width * _pixelScale);
            uint actualHeight = (uint)(height * _pixelScale);
            _dxgiSwapChain.ResizeBuffers(2, (int)actualWidth, (int)actualHeight, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            using (Texture2D backBufferTexture = _dxgiSwapChain.GetBackBuffer<Texture2D>(0))
            {
                Texture2D depthBufferTexture = null;
                if (_depthFormat != null)
                {
                    TextureDescription depthDesc = new TextureDescription(
                        actualWidth, actualHeight, 1, 1, 1,
                        _depthFormat.Value,
                        TextureUsage.DepthStencil,
                        TextureType.Texture2D);
                    _depthTexture = new D3D11Texture(_device, ref depthDesc);
                }

                D3D11Texture backBufferVdTexture = new D3D11Texture(backBufferTexture);
                D3D11Texture depthVdTexture = depthBufferTexture != null
                    ? new D3D11Texture(depthBufferTexture)
                    : null;
                FramebufferDescription desc = new FramebufferDescription(depthVdTexture, backBufferVdTexture);
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

        public override void Dispose()
        {
            _depthTexture?.Dispose();
            _framebuffer.Dispose();
            _dxgiSwapChain.Dispose();
        }
    }
}