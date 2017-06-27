using System;

namespace Veldrid.Graphics
{
    internal struct SamplerStateCacheKey : IEquatable<SamplerStateCacheKey>
    {
        public SamplerAddressMode AddressU { get; }
        public SamplerAddressMode AddressV { get; }
        public SamplerAddressMode AddressW { get; }
        public SamplerFilter Filter { get; }
        public int MaximumAnisotropy { get; }
        public RgbaFloat BorderColor { get; }
        public DepthComparison Comparison { get; }
        public int MinimumLod { get; }
        public int MaximumLod { get; }
        public int LodBias { get; }

        public SamplerStateCacheKey(
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
            AddressU = addressU;
            AddressV = addressV;
            AddressW = addressW;
            Filter = filter;
            MaximumAnisotropy = maxAnisotropy;
            BorderColor = borderColor;
            Comparison = comparison;
            MinimumLod = minimumLod;
            MaximumLod = maximumLod;
            LodBias = lodBias;
        }

        public bool Equals(SamplerStateCacheKey other)
        {
            return AddressU == other.AddressU && AddressV == other.AddressV && AddressW == other.AddressW
                && Filter == other.Filter && MaximumAnisotropy == other.MaximumAnisotropy && BorderColor.Equals(other.BorderColor)
                && Comparison == other.Comparison && MinimumLod.Equals(other.MinimumLod) && MaximumLod.Equals(other.MaximumLod)
                && LodBias.Equals(other.LodBias);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(AddressU.GetHashCode(), AddressV.GetHashCode(), AddressW.GetHashCode(),
                HashHelper.Combine(Filter.GetHashCode(), MaximumAnisotropy.GetHashCode(), BorderColor.GetHashCode(),
                    HashHelper.Combine(Comparison.GetHashCode(), MinimumLod.GetHashCode(), MaximumLod.GetHashCode(), LodBias.GetHashCode())));
        }
    }
}
