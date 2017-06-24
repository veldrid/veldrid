using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DBlendState : BlendState, IDisposable
    {
        private readonly Device _device;
        private readonly SharpDX.Direct3D11.BlendState _deviceBlendState;

        public D3DBlendState(
            Device device, bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc,
            RgbaFloat blendFactor)
        {
            _device = device;
            IsBlendEnabled = isBlendEnabled;
            SourceAlphaBlend = srcAlpha;
            DestinationAlphaBlend = destAlpha;
            AlphaBlendFunction = alphaBlendFunc;
            SourceColorBlend = srcColor;
            DestinationColorBlend = destColor;
            ColorBlendFunction = colorBlendFunc;
            BlendFactor = blendFactor;

            var desc = new BlendStateDescription();
            desc.RenderTarget[0].SourceAlphaBlend = D3DFormats.VeldridToD3DBlend(SourceAlphaBlend);
            desc.RenderTarget[0].DestinationAlphaBlend = D3DFormats.VeldridToD3DBlend(DestinationAlphaBlend);
            desc.RenderTarget[0].AlphaBlendOperation = D3DFormats.VeldridToD3DBlendFunction(AlphaBlendFunction);
            desc.RenderTarget[0].SourceBlend = D3DFormats.VeldridToD3DBlend(SourceColorBlend);
            desc.RenderTarget[0].DestinationBlend = D3DFormats.VeldridToD3DBlend(DestinationColorBlend);
            desc.RenderTarget[0].BlendOperation = D3DFormats.VeldridToD3DBlendFunction(ColorBlendFunction);
            desc.RenderTarget[0].IsBlendEnabled = isBlendEnabled;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            _deviceBlendState = new SharpDX.Direct3D11.BlendState(_device, desc);
        }

        public bool IsBlendEnabled { get; }

        public RgbaFloat BlendFactor { get; }

        public Blend SourceAlphaBlend { get; }
        public Blend DestinationAlphaBlend { get; }
        public BlendFunction AlphaBlendFunction { get; }

        public Blend SourceColorBlend { get; }
        public Blend DestinationColorBlend { get; }
        public BlendFunction ColorBlendFunction { get; }

        public unsafe void Apply()
        {
            _device.ImmediateContext.OutputMerger.SetBlendState(
                _deviceBlendState,
                new RawColor4(BlendFactor.R, BlendFactor.G, BlendFactor.B, BlendFactor.A),
                0xffffffff);
        }

        public void Dispose()
        {
            _deviceBlendState.Dispose();
        }
    }
}
