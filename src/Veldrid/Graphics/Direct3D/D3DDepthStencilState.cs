using SharpDX.Direct3D11;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DDepthStencilState : DepthStencilState, IDisposable
    {
        private readonly Device _device;
        private readonly SharpDX.Direct3D11.DepthStencilState _deviceState;

        public D3DDepthStencilState(Device device, bool isDepthEnabled, DepthComparison comparison, bool isDepthWriteEnabled)
        {
            _device = device;
            IsDepthEnabled = IsDepthEnabled;
            IsDepthWriteEnabled = isDepthWriteEnabled;
            DepthComparison = comparison;

            DepthStencilStateDescription desc = DepthStencilStateDescription.Default();
            desc.DepthComparison = D3DFormats.ConvertDepthComparison(comparison);
            desc.IsDepthEnabled = isDepthEnabled;
            desc.DepthWriteMask = isDepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero;

            _deviceState = new SharpDX.Direct3D11.DepthStencilState(device, desc);
        }

        public bool IsDepthEnabled { get; }

        public bool IsDepthWriteEnabled { get; }

        public DepthComparison DepthComparison { get; }

        public void Apply()
        {
            _device.ImmediateContext.OutputMerger.SetDepthStencilState(_deviceState);
        }

        public void Dispose()
        {
            _deviceState.Dispose();
        }
    }
}
