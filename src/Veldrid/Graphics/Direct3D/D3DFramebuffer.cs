using System;
using SharpDX.Direct3D11;
using System.Diagnostics;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DFramebuffer : Framebuffer, IDisposable
    {
        private readonly Device _device;
        private int _width;
        private int _height;

        public DepthStencilView DepthStencilView { get; private set; }
        public RenderTargetView RenderTargetView { get; private set; }
        public D3DTexture2D RenderTargetTexture { get; private set; }
        public D3DTexture2D DepthTexture { get; private set; }

        DeviceTexture2D Framebuffer.ColorTexture
        {
            get
            {
                return RenderTargetTexture;
            }

            set
            {
                Debug.Assert(value is D3DTexture2D);
                RenderTargetTexture = (D3DTexture2D)value;
                RenderTargetView = new RenderTargetView(_device, RenderTargetTexture.DeviceTexture);
                _width = value.Width;
                _height = value.Height;
            }
        }

        DeviceTexture2D Framebuffer.DepthTexture
        {
            get
            {
                return DepthTexture;
            }

            set
            {
                Debug.Assert(value is D3DTexture2D);
                DepthTexture = (D3DTexture2D)value;
                DepthStencilView = new DepthStencilView(_device, DepthTexture.DeviceTexture, new DepthStencilViewDescription()
                {
                    Format = SharpDX.DXGI.Format.D16_UNorm,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Flags = DepthStencilViewFlags.None,
                });
            }
        }

        public int Width => _width;

        public int Height => _height;

        public D3DFramebuffer(Device device)
        {
            _device = device;
        }

        public D3DFramebuffer(Device device, D3DTexture2D colorTexture, D3DTexture2D depthTexture)
            : this(device, colorTexture, depthTexture, colorTexture.Width, colorTexture.Height)
        {
        }

        public D3DFramebuffer(Device device, D3DTexture2D colorTexture, D3DTexture2D depthTexture, int width, int height)
        {
            _device = device;
            RenderTargetView = new RenderTargetView(device, colorTexture.DeviceTexture);
            DepthStencilView = new DepthStencilView(device, depthTexture.DeviceTexture);
            RenderTargetTexture = colorTexture;
            DepthTexture = depthTexture;
            _width = width;
            _height = height;
        }

        public void Apply()
        {
            _device.ImmediateContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);
        }

        public void Dispose()
        {
            RenderTargetView?.Dispose();
            DepthStencilView?.Dispose();
        }
    }
}