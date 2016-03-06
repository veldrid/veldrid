using System;
using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DFramebuffer : Framebuffer, IDisposable
    {
        private readonly Device _device;

        public DepthStencilView DepthStencilView { get; }
        public RenderTargetView RenderTargetView { get; }
        public D3DTexture RenderTargetTexture { get; }
        public D3DTexture DepthTexture { get; }

        DeviceTexture Framebuffer.ColorTexture => RenderTargetTexture;
        DeviceTexture Framebuffer.DepthTexture => DepthTexture;

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
            RenderTargetView.Dispose();
            DepthStencilView.Dispose();
        }
    }
}