using System;

namespace Veldrid.Graphics
{
    public interface SamplerState : IDisposable
    {
        SamplerAddressMode AddressU { get; }
        SamplerAddressMode AddressV { get; }
        SamplerAddressMode AddressW { get; }
        SamplerFilter Filter { get; }
        int MaximumAnisotropy { get; }
        RgbaFloat BorderColor { get; }
        DepthComparison Comparison { get; }
        int MinimumLod { get; }
        int MaximumLod { get; }
        int LodBias { get; }
    }
}
