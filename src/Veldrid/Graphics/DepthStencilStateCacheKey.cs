using System;

namespace Veldrid.Graphics
{
    internal struct DepthStencilStateCacheKey : IEquatable<DepthStencilStateCacheKey>
    {
        public bool IsDepthEnabled { get; }
        public bool IsDepthWriteEnabled { get; }
        public DepthComparison Comparison { get; }

        public DepthStencilStateCacheKey(bool isDepthEnabled, bool isDepthWriteEnabled, DepthComparison comparison)
        {
            IsDepthEnabled = isDepthEnabled;
            IsDepthWriteEnabled = isDepthWriteEnabled;
            Comparison = comparison;
        }

        public bool Equals(DepthStencilStateCacheKey other)
        {
            return IsDepthEnabled == other.IsDepthEnabled
                && IsDepthWriteEnabled == other.IsDepthWriteEnabled
                && Comparison == other.Comparison;
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(
                IsDepthEnabled.GetHashCode(),
                HashHelper.Combine(IsDepthWriteEnabled.GetHashCode(), Comparison.GetHashCode()));
        }
    }
}
