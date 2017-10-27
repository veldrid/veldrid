using System;

namespace Veldrid
{
    public struct DepthStencilStateDescription : IEquatable<DepthStencilStateDescription>
    {
        public bool DepthTestEnabled;
        public bool DepthWriteEnabled;
        public DepthComparisonKind ComparisonKind;

        public DepthStencilStateDescription(bool depthTestEnabled, bool depthWriteEnabled, DepthComparisonKind comparisonKind)
        {
            DepthTestEnabled = depthTestEnabled;
            DepthWriteEnabled = depthWriteEnabled;
            ComparisonKind = comparisonKind;
        }

        public static readonly DepthStencilStateDescription LessEqual = new DepthStencilStateDescription
        {
            DepthTestEnabled = true,
            DepthWriteEnabled = true,
            ComparisonKind = DepthComparisonKind.LessEqual
        };

        public bool Equals(DepthStencilStateDescription other)
        {
            return DepthTestEnabled.Equals(other.DepthTestEnabled) && DepthWriteEnabled.Equals(other.DepthWriteEnabled)
                && ComparisonKind == other.ComparisonKind;
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(DepthTestEnabled.GetHashCode(), DepthWriteEnabled.GetHashCode(), ComparisonKind.GetHashCode());
        }
    }
}