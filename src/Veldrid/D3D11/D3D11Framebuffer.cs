using SharpDX.Direct3D11;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11Framebuffer : Framebuffer
    {
        public RenderTargetView[] RenderTargetViews { get; }
        public DepthStencilView DepthStencilView { get; }

        internal bool IsSwapchainFramebuffer { get; set; }

        public D3D11Framebuffer(Device device, ref FramebufferDescription description)
            : base(description.DepthTarget, description.ColorTargets)
        {
            if (description.DepthTarget != null)
            {
                D3D11Texture d3dDepthTarget = Util.AssertSubtype<Texture, D3D11Texture>(description.DepthTarget);
                DepthStencilViewDimension dimension = d3dDepthTarget.SampleCount == TextureSampleCount.Count1
                    ? DepthStencilViewDimension.Texture2D
                    : DepthStencilViewDimension.Texture2DMultisampled;
                DepthStencilViewDescription dsvDesc = new DepthStencilViewDescription()
                {
                    Dimension = dimension,
                    Format = D3D11Formats.GetDepthFormat(d3dDepthTarget.Format)
                };
                DepthStencilView = new DepthStencilView(device, d3dDepthTarget.DeviceTexture, dsvDesc);
            }

            if (description.ColorTargets != null && description.ColorTargets.Length > 0)
            {
                RenderTargetViews = new RenderTargetView[description.ColorTargets.Length];
                for (int i = 0; i < RenderTargetViews.Length; i++)
                {
                    D3D11Texture d3dColorTarget = Util.AssertSubtype<Texture, D3D11Texture>(description.ColorTargets[i]);
                    RenderTargetViewDimension dimension = d3dColorTarget.SampleCount == TextureSampleCount.Count1
                        ? RenderTargetViewDimension.Texture2D
                        : RenderTargetViewDimension.Texture2DMultisampled;
                    RenderTargetViewDescription rtvDesc = new RenderTargetViewDescription
                    {
                        Dimension = dimension,
                        Format = D3D11Formats.ToDxgiFormat(d3dColorTarget.Format, false),
                    };
                    RenderTargetViews[i] = new RenderTargetView(device, d3dColorTarget.DeviceTexture, rtvDesc);
                }
            }
            else
            {
                RenderTargetViews = Array.Empty<RenderTargetView>();
            }
        }

        public override void Dispose()
        {
            DepthStencilView?.Dispose();
            foreach (RenderTargetView rtv in RenderTargetViews)
            {
                rtv.Dispose();
            }
        }
    }
}