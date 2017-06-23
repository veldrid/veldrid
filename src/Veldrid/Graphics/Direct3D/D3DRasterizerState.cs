using System;
using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DRasterizerState : RasterizerState, IDisposable
    {
        private readonly Device _device;
        private readonly SharpDX.Direct3D11.RasterizerState _deviceState;

        public FaceCullingMode CullMode { get; }
        public TriangleFillMode FillMode { get; }
        public bool IsDepthClipEnabled { get; }
        public bool IsScissorTestEnabled { get; }

        public D3DRasterizerState(
            Device device,
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool depthClipEnabled,
            bool scissorTestEnabled)
        {
            _device = device;
            CullMode = cullMode;
            FillMode = fillMode;
            IsDepthClipEnabled = depthClipEnabled;
            IsScissorTestEnabled = scissorTestEnabled;

            var desc = new RasterizerStateDescription()
            {
                IsDepthClipEnabled = IsDepthClipEnabled,
                IsScissorEnabled = IsScissorTestEnabled,
                CullMode = D3DFormats.VeldridToD3DFaceCullingMode(cullMode),
                FillMode = D3DFormats.VeldridToD3DTriangleFillMode(fillMode)
            };

            _deviceState = new SharpDX.Direct3D11.RasterizerState(device, desc);
        }

        public void Apply()
        {
            _device.ImmediateContext.Rasterizer.State = _deviceState;
        }

        public void Dispose()
        {
            _deviceState.Dispose();
        }
    }
}
