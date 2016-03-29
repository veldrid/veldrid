using System;
using SharpDX.Direct3D11;
using System.Diagnostics;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DFramebuffer : Framebuffer, IDisposable
    {
        private readonly Device _device;

        public DepthStencilView DepthStencilView { get; private set; }
        public RenderTargetView RenderTargetView { get; private set; }
        public D3DTexture RenderTargetTexture { get; private set; }
        public D3DTexture DepthTexture { get; private set; }

        DeviceTexture Framebuffer.ColorTexture
        {
            get
            {
                return RenderTargetTexture;
            }

            set
            {
                Debug.Assert(value is D3DTexture);
                RenderTargetTexture = (D3DTexture)value;
                RenderTargetView = new RenderTargetView(_device, RenderTargetTexture.DeviceTexture);
            }
        }

        DeviceTexture Framebuffer.DepthTexture
        {
            get
            {
                return DepthTexture;
            }

            set
            {
                Debug.Assert(value is D3DTexture);
                DepthTexture = (D3DTexture)value;
                //DepthStencilView = new DepthStencilView(_device, DepthTexture.DeviceTexture);
                DepthStencilView = new DepthStencilView(_device, DepthTexture.DeviceTexture, new DepthStencilViewDescription()
                {
                     Format = SharpDX.DXGI.Format.D16_UNorm,
                      Dimension = DepthStencilViewDimension.Texture2D,
                       Flags = DepthStencilViewFlags.None,

                });
            }
        }

        public D3DFramebuffer(Device device)
        {
            _device = device;
        }

        public D3DFramebuffer(Device device, D3DTexture colorTexture, D3DTexture depthTexture)
        {
            _device = device;
            RenderTargetView = new RenderTargetView(device, colorTexture.DeviceTexture);
            DepthStencilView = new DepthStencilView(device, depthTexture.DeviceTexture);
            RenderTargetTexture = colorTexture;
            DepthTexture = depthTexture;
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