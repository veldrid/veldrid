using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DSamplerState : SamplerState
    {
        private readonly SharpDX.Direct3D11.SamplerState _d3dSamplerState;

        public D3DSamplerState(Device device, SamplerStateDescription ssd)
        {
            _d3dSamplerState = new SharpDX.Direct3D11.SamplerState(device, ssd);
        }

        public D3DSamplerState(
            Device device,
            SamplerAddressMode addressU, 
            SamplerAddressMode addressV, 
            SamplerAddressMode addressW, 
            SamplerFilter filter, 
            int maxAnisotropy, 
            RgbaFloat borderColor, 
            DepthComparison comparison, 
            int minimumLod, 
            int maximumLod, 
            int lodBias)
        {
            SamplerStateDescription ssd = new SamplerStateDescription();
            ssd.AddressU = D3DFormats.VeldridToD3DSamplerAddressMode(addressU);
            ssd.AddressV = D3DFormats.VeldridToD3DSamplerAddressMode(addressV);
            ssd.AddressW = D3DFormats.VeldridToD3DSamplerAddressMode(addressW);
            ssd.Filter = D3DFormats.VeldridToD3DSamplerFilter(filter);
            ssd.MaximumAnisotropy = maxAnisotropy;
            ssd.BorderColor = new RawColor4(borderColor.R, borderColor.G, borderColor.B, borderColor.A);
            ssd.ComparisonFunction = D3DFormats.VeldridToD3DDepthComparison(comparison);
            ssd.MinimumLod = minimumLod;
            ssd.MaximumLod = maximumLod;
            ssd.MipLodBias = lodBias;
            _d3dSamplerState = new SharpDX.Direct3D11.SamplerState(device, ssd);
        }

        public SharpDX.Direct3D11.SamplerState SamplerState => _d3dSamplerState;

        public SamplerAddressMode AddressU => D3DFormats.D3DToVeldridSamplerAddressMode(_d3dSamplerState.Description.AddressU);

        public SamplerAddressMode AddressV => D3DFormats.D3DToVeldridSamplerAddressMode(_d3dSamplerState.Description.AddressV);

        public SamplerAddressMode AddressW => D3DFormats.D3DToVeldridSamplerAddressMode(_d3dSamplerState.Description.AddressW);

        public SamplerFilter Filter => D3DFormats.D3DToVeldridSamplerFilter(_d3dSamplerState.Description.Filter);

        public int MaximumAnisotropy => _d3dSamplerState.Description.MaximumAnisotropy;

        public RgbaFloat BorderColor
        {
            get
            {
                RawColor4 rawColor = _d3dSamplerState.Description.BorderColor;
                return new RgbaFloat(rawColor.R, rawColor.G, rawColor.B, rawColor.A);
            }
        }

        public DepthComparison Comparison => D3DFormats.D3DToVeldridDepthComparison(_d3dSamplerState.Description.ComparisonFunction);

        public int MinimumLod => (int)_d3dSamplerState.Description.MinimumLod;
        public int MaximumLod => (int)_d3dSamplerState.Description.MinimumLod;
        public int LodBias => (int)_d3dSamplerState.Description.MipLodBias;

        public void Dispose()
        {
            _d3dSamplerState.Dispose();
        }
    }
}
