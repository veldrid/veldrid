using System;
using SharpDX.Direct3D11;
using System.Diagnostics;
using System.Collections.Generic;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DFramebuffer : Framebuffer, IDisposable
    {
        private readonly Device _device;
        private int _width;
        private int _height;
        private RenderTargetView[] _renderTargetViews = new RenderTargetView[RenderContext.MaxRenderTargets];
        private D3DTexture2D[] _colorTextures = new D3DTexture2D[RenderContext.MaxRenderTargets];

        public DepthStencilView DepthStencilView { get; private set; }
        public D3DTexture2D DepthTexture { get; private set; }
        public IReadOnlyList<RenderTargetView> RenderTargetViews => _renderTargetViews;

        public int Width => _width;

        public int Height => _height;

        DeviceTexture2D Framebuffer.DepthTexture
        {
            get => DepthTexture;
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

        DeviceTexture2D Framebuffer.ColorTexture
        {
            get => GetColorTexture(0);
            set => AttachColorTexture(0, value);
        }

        public DeviceTexture2D GetColorTexture(int index) => _colorTextures[index];

        public void AttachColorTexture(int index, DeviceTexture2D texture)
        {
            _colorTextures[index] = (D3DTexture2D)texture;
            _renderTargetViews[index] = new RenderTargetView(_device, _colorTextures[index].DeviceTexture);
            if (index == 0)
            {
                _width = texture.Width;
                _height = texture.Height;
            }
        }

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
            _colorTextures[0] = colorTexture;
            _renderTargetViews[0] = new RenderTargetView(device, colorTexture.DeviceTexture);
            DepthStencilViewDescription dsvd = new DepthStencilViewDescription();
            dsvd.Format = SharpDX.DXGI.Format.D16_UNorm;
            dsvd.Dimension = DepthStencilViewDimension.Texture2D;
            DepthStencilView = new DepthStencilView(device, depthTexture.DeviceTexture, dsvd);
            DepthTexture = depthTexture;
            _width = width;
            _height = height;
        }

        public void Apply()
        {
            _device.ImmediateContext.OutputMerger.SetTargets(DepthStencilView, _renderTargetViews);
        }

        public void Dispose()
        {
            foreach (var rtv in _renderTargetViews)
            {
                rtv?.Dispose();
            }

            DepthStencilView?.Dispose();
        }
    }
}