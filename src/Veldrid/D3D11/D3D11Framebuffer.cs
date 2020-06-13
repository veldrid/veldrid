using SharpDX.Direct3D11;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11Framebuffer : Framebuffer
    {
        private string _name;
        private bool _disposed;

        public RenderTargetView[] RenderTargetViews { get; }
        public DepthStencilView DepthStencilView { get; }

        // Only non-null if this is the Framebuffer for a Swapchain.
        internal D3D11Swapchain Swapchain { get; set; }

        public override bool IsDisposed => _disposed;

        public D3D11Framebuffer(Device device, ref FramebufferDescription description)
            : base(description.DepthTarget, description.ColorTargets)
        {
            ValidationHelpers.ValidateFramebufferDescription(description);

            if (description.DepthTarget != null)
            {
                D3D11Texture d3dDepthTarget = Util.AssertSubtype<Texture, D3D11Texture>(description.DepthTarget.Value.Target);
                DepthStencilViewDescription dsvDesc = new DepthStencilViewDescription()
                {
                    Format = D3D11Formats.GetDepthFormat(d3dDepthTarget.Format),
                };
                bool isCubemap = (d3dDepthTarget.Usage & TextureUsage.Cubemap) != 0;
                bool isTextureArray = d3dDepthTarget.ArrayLayers > 1;

                if (!isTextureArray && !isCubemap)
                {
                    if (d3dDepthTarget.SampleCount == TextureSampleCount.Count1)
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2D;
                        dsvDesc.Texture2D.MipSlice = (int)description.DepthTarget.Value.MipLevel;
                    }
                    else
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2DMultisampled;
                    }
                }
                else
                {
                    uint arraySize = 1;
                    uint arrayLayer = description.DepthTarget.Value.ArrayLayer;

                    if (description.DepthTarget.Value.LayeredTarget)
                    {
                        arraySize = d3dDepthTarget.ArrayLayers;
                        arrayLayer = 0;

                        if (isCubemap)
                        {
                            arraySize *= 6;
                        }
                    }

                    if (d3dDepthTarget.SampleCount == TextureSampleCount.Count1)
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2DArray;
                        dsvDesc.Texture2DArray.FirstArraySlice = (int)arrayLayer;
                        dsvDesc.Texture2DArray.ArraySize = (int)arraySize;
                        dsvDesc.Texture2DArray.MipSlice = (int)description.DepthTarget.Value.MipLevel;
                    }
                    else
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2DMultisampledArray;
                        dsvDesc.Texture2DMSArray.FirstArraySlice = (int)arrayLayer;
                        dsvDesc.Texture2DMSArray.ArraySize = (int)arraySize;
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
                    bool isCubemap = (d3dColorTarget.Usage & TextureUsage.Cubemap) != 0;
                    bool isTextureArray = d3dColorTarget.ArrayLayers > 1;

                    if (isTextureArray || isCubemap)
                    {
                        uint arraySize = 1;
                        uint arrayLayer = description.ColorTargets[i].ArrayLayer;

                        if (description.ColorTargets[i].LayeredTarget)
                        {
                            arraySize = d3dColorTarget.ArrayLayers;
                            arrayLayer = 0;

                            if (isCubemap)
                            {
                                arraySize *= 6;
                            }
                        }

                        if (d3dColorTarget.SampleCount == TextureSampleCount.Count1)
                        {
                            rtvDesc.Dimension = RenderTargetViewDimension.Texture2DArray;
                            rtvDesc.Texture2DArray = new RenderTargetViewDescription.Texture2DArrayResource
                            {
                                ArraySize = (int)arraySize,
                                FirstArraySlice = (int)arrayLayer,
                                MipSlice = (int)description.ColorTargets[i].MipLevel
                            };
                        }
                        else
                        {
                            rtvDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampledArray;
                            rtvDesc.Texture2DMSArray = new RenderTargetViewDescription.Texture2DMultisampledArrayResource
                            {
                                ArraySize = (int)arraySize,
                                FirstArraySlice = (int)arrayLayer
                            };
                        }
                    }
                    else
                    {
                        if (d3dColorTarget.SampleCount == TextureSampleCount.Count1)
                        {
                            rtvDesc.Dimension = RenderTargetViewDimension.Texture2D;
                            rtvDesc.Texture2D.MipSlice = (int)description.ColorTargets[i].MipLevel;
                        }
                        else
                        {
                            rtvDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampled;
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
            if (!_disposed)
            {
                DepthStencilView?.Dispose();
                foreach (RenderTargetView rtv in RenderTargetViews)
                {
                    rtv.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
