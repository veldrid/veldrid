using SharpDX.Direct3D11;

namespace Vd2.D3D11
{
    internal class D3D11Sampler : Sampler
    {
        public SamplerState DeviceSampler { get; }

        public D3D11Sampler(Device device, ref SamplerDescription description)
        {
            Comparison comparision = description.ComparisonKind == null ? Comparison.Never : D3D11Formats.VdToD3D11DepthComparison(description.ComparisonKind.Value);
            SamplerStateDescription samplerStateDesc = new SamplerStateDescription
            {
                AddressU = D3D11Formats.VdToD3D11AddressMode(description.AddressModeU),
                AddressV = D3D11Formats.VdToD3D11AddressMode(description.AddressModeV),
                AddressW = D3D11Formats.VdToD3D11AddressMode(description.AddressModeW),
                Filter = D3D11Formats.ToD3D11Filter(description.Filter, description.ComparisonKind.HasValue),
                MinimumLod = description.MinimumLod,
                MaximumLod = description.MaximumLod,
                MaximumAnisotropy = (int)description.MaximumAnisotropy,
                ComparisonFunction = comparision,
                MipLodBias = description.LodBias
            };

            DeviceSampler = new SamplerState(device, samplerStateDesc);
        }
    }
}