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
                DepthStencilViewDescription dsvDesc = new DepthStencilViewDescription()
                {
                    Dimension = DepthStencilViewDimension.Texture2D,
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
                    RenderTargetViewDescription rtvDesc = new RenderTargetViewDescription
                    {
                        Format = D3D11Formats.ToDxgiFormat(d3dColorTarget.Format, false),
                        Dimension = RenderTargetViewDimension.Texture2D,
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