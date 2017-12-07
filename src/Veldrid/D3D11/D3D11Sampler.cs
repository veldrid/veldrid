using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace Veldrid.D3D11
{
    internal class D3D11Sampler : Sampler
    {
        private string _name;

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
                MipLodBias = description.LodBias,
                BorderColor = ToRawColor4(description.BorderColor)
            };

            DeviceSampler = new SamplerState(device, samplerStateDesc);
        }

        private static RawColor4 ToRawColor4(SamplerBorderColor borderColor)
        {
            switch (borderColor)
            {
                case SamplerBorderColor.TransparentBlack:
                    return new RawColor4(0, 0, 0, 0);
                case SamplerBorderColor.OpaqueBlack:
                    return new RawColor4(0, 0, 0, 1);
                case SamplerBorderColor.OpaqueWhite:
                    return new RawColor4(1, 1, 1, 1);
                default:
                    throw Illegal.Value<SamplerBorderColor>();
            }
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                DeviceSampler.DebugName = value;
            }
        }

        public override void Dispose()
        {
            DeviceSampler.Dispose();
        }
    }
}