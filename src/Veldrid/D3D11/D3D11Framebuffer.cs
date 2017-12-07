using SharpDX.Direct3D11;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11Framebuffer : Framebuffer
    {
        private string _name;

        public RenderTargetView[] RenderTargetViews { get; }
        public DepthStencilView DepthStencilView { get; }

        internal bool IsSwapchainFramebuffer { get; set; }

        public D3D11Framebuffer(Device device, ref FramebufferDescription description)
            : base(description.DepthTarget, description.ColorTargets)
        {
            if (description.DepthTarget != null)
            {
                D3D11Texture d3dDepthTarget = Util.AssertSubtype<Texture, D3D11Texture>(description.DepthTarget.Value.Target);
                DepthStencilViewDescription dsvDesc = new DepthStencilViewDescription()
                {
                    Format = D3D11Formats.GetDepthFormat(d3dDepthTarget.Format),
                };
                if (d3dDepthTarget.ArrayLayers == 1)
                {
                    if (d3dDepthTarget.SampleCount == TextureSampleCount.Count1)
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2D;
                    }
                    else
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2DMultisampled;
                    }
                }
                else
                {
                    if (d3dDepthTarget.SampleCount == TextureSampleCount.Count1)
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2DArray;
                        dsvDesc.Texture2DArray.FirstArraySlice = (int)description.DepthTarget.Value.ArrayLayer;
                        dsvDesc.Texture2DArray.ArraySize = 1;
                    }
                    else
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2DMultisampledArray;
                        dsvDesc.Texture2DMSArray.FirstArraySlice = (int)description.DepthTarget.Value.ArrayLayer;
                        dsvDesc.Texture2DMSArray.ArraySize = 1;
                    }
                }

                DepthStencilView = new DepthStencilView(device, d3dDepthTarget.DeviceTexture, dsvDesc);
            }

            if (description.ColorTargets != null && description.ColorTargets.Length > 0)
            {
                RenderTargetViews = new RenderTargetView[description.ColorTargets.Length];
                for (int i = 0; i < RenderTargetViews.Length; i++)
                {
                    D3D11Texture d3dColorTarget = Util.AssertSubtype<Texture, D3D11Texture>(description.ColorTargets[i].Target);
                    RenderTargetViewDescription rtvDesc = new RenderTargetViewDescription
                    {
                        Format = D3D11Formats.ToDxgiFormat(d3dColorTarget.Format, false),
                    };
                    if (d3dColorTarget.ArrayLayers == 1)
                    {
                        if (d3dColorTarget.SampleCount == TextureSampleCount.Count1)
                        {
                            rtvDesc.Dimension = RenderTargetViewDimension.Texture2D;
                        }
                        else
                        {
                            rtvDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampled;
                        }
                    }
                    else
                    {
                        if (d3dColorTarget.SampleCount == TextureSampleCount.Count1)
                        {
                            rtvDesc.Dimension = RenderTargetViewDimension.Texture2DArray;
                            rtvDesc.Texture2DArray = new RenderTargetViewDescription.Texture2DArrayResource
                            {
                                ArraySize = 1,
                                FirstArraySlice = (int)description.ColorTargets[i].ArrayLayer
                            };
                        }
                        else
                        {
                            rtvDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampledArray;
                            rtvDesc.Texture2DMSArray = new RenderTargetViewDescription.Texture2DMultisampledArrayResource
                            {
                                ArraySize = 1,
                                FirstArraySlice = (int)description.ColorTargets[i].ArrayLayer
                            };
                        }
                    }
                    RenderTargetViews[i] = new RenderTargetView(device, d3dColorTarget.DeviceTexture, rtvDesc);
                }
            }
            else
            {
                RenderTargetViews = Array.Empty<RenderTargetView>();
            }
        }

        public override string Name
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
            DepthStencilView?.Dispose();
            foreach (RenderTargetView rtv in RenderTargetViews)
            {
                rtv.Dispose();
            }
        }
    }
}