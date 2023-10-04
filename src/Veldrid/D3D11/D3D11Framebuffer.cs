using System;
using Vortice.Direct3D11;

namespace Veldrid.D3D11
{
    internal sealed class D3D11Framebuffer : Framebuffer
    {
        private string? _name;
        private bool _disposed;

        public ID3D11RenderTargetView[] RenderTargetViews { get; }
        public ID3D11DepthStencilView? DepthStencilView { get; }

        // Only non-null if this is the Framebuffer for a Swapchain.
        internal D3D11Swapchain? Swapchain { get; set; }

        public override bool IsDisposed => _disposed;

        public D3D11Framebuffer(ID3D11Device device, in FramebufferDescription description)
            : base(description.DepthTarget, description.ColorTargets)
        {
            if (description.DepthTarget != null)
            {
                FramebufferAttachmentDescription depthTarget = description.DepthTarget.GetValueOrDefault();

                D3D11Texture d3dDepthTarget = Util.AssertSubtype<Texture, D3D11Texture>(depthTarget.Target);
                DepthStencilViewDescription dsvDesc = new()
                {
                    Format = D3D11Formats.GetDepthFormat(d3dDepthTarget.Format),
                };
                if (d3dDepthTarget.ArrayLayers == 1)
                {
                    if (d3dDepthTarget.SampleCount == TextureSampleCount.Count1)
                    {
                        dsvDesc.ViewDimension = DepthStencilViewDimension.Texture2D;
                        dsvDesc.Texture2D.MipSlice = (int)depthTarget.MipLevel;
                    }
                    else
                    {
                        dsvDesc.ViewDimension = DepthStencilViewDimension.Texture2DMultisampled;
                    }
                }
                else
                {
                    if (d3dDepthTarget.SampleCount == TextureSampleCount.Count1)
                    {
                        dsvDesc.ViewDimension = DepthStencilViewDimension.Texture2DArray;
                        dsvDesc.Texture2DArray.FirstArraySlice = (int)depthTarget.ArrayLayer;
                        dsvDesc.Texture2DArray.ArraySize = 1;
                        dsvDesc.Texture2DArray.MipSlice = (int)depthTarget.MipLevel;
                    }
                    else
                    {
                        dsvDesc.ViewDimension = DepthStencilViewDimension.Texture2DMultisampledArray;
                        dsvDesc.Texture2DMSArray.FirstArraySlice = (int)depthTarget.ArrayLayer;
                        dsvDesc.Texture2DMSArray.ArraySize = 1;
                    }
                }

                DepthStencilView = device.CreateDepthStencilView(d3dDepthTarget.DeviceTexture, dsvDesc);
            }

            ReadOnlySpan<FramebufferAttachmentDescription> colorTargets = description.ColorTargets;
            if (colorTargets.Length > 0)
            {
                RenderTargetViews = new ID3D11RenderTargetView[colorTargets.Length];
                for (int i = 0; i < RenderTargetViews.Length; i++)
                {
                    D3D11Texture d3dColorTarget = Util.AssertSubtype<Texture, D3D11Texture>(colorTargets[i].Target);
                    RenderTargetViewDescription rtvDesc = new()
                    {
                        Format = D3D11Formats.ToDxgiFormat(d3dColorTarget.Format, false),
                    };
                    if (d3dColorTarget.ArrayLayers > 1 || (d3dColorTarget.Usage & TextureUsage.Cubemap) != 0)
                    {
                        if (d3dColorTarget.SampleCount == TextureSampleCount.Count1)
                        {
                            rtvDesc.ViewDimension = RenderTargetViewDimension.Texture2DArray;
                            rtvDesc.Texture2DArray = new Texture2DArrayRenderTargetView
                            {
                                ArraySize = 1,
                                FirstArraySlice = (int)colorTargets[i].ArrayLayer,
                                MipSlice = (int)colorTargets[i].MipLevel
                            };
                        }
                        else
                        {
                            rtvDesc.ViewDimension = RenderTargetViewDimension.Texture2DMultisampledArray;
                            rtvDesc.Texture2DMSArray = new Texture2DMultisampledArrayRenderTargetView
                            {
                                ArraySize = 1,
                                FirstArraySlice = (int)colorTargets[i].ArrayLayer
                            };
                        }
                    }
                    else
                    {
                        if (d3dColorTarget.SampleCount == TextureSampleCount.Count1)
                        {
                            rtvDesc.ViewDimension = RenderTargetViewDimension.Texture2D;
                            rtvDesc.Texture2D.MipSlice = (int)colorTargets[i].MipLevel;
                        }
                        else
                        {
                            rtvDesc.ViewDimension = RenderTargetViewDimension.Texture2DMultisampled;
                        }
                    }
                    RenderTargetViews[i] = device.CreateRenderTargetView(d3dColorTarget.DeviceTexture, rtvDesc);
                }
            }
            else
            {
                RenderTargetViews = Array.Empty<ID3D11RenderTargetView>();
            }
        }

        public override string? Name
        {
            get => _name;
            set
            {
                _name = value;

                for (int i = 0; i < RenderTargetViews.Length; i++)
                {
                    RenderTargetViews[i].DebugName = value + "_RTV" + i;
                }

                if (DepthStencilView != null)
                {
                    DepthStencilView.DebugName = value + "_DSV";
                }
            }
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                DepthStencilView?.Dispose();
                foreach (ID3D11RenderTargetView rtv in RenderTargetViews)
                {
                    rtv.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
