using System;

namespace Veldrid.Graphics
{
    internal struct RasterizerStateCacheKey : IEquatable<RasterizerStateCacheKey>
    {
        public FaceCullingMode CullMode { get; }
        public TriangleFillMode FillMode { get; }
        public bool IsDepthClipEnabled { get; }
        public bool IsScissorTestEnabled { get; }

        public RasterizerStateCacheKey(
            FaceCullingMode cullMode, 
            TriangleFillMode fillMode, 
            bool isDepthClipEnabled, 
            bool isScissorTestEnabled)
        {
            CullMode = cullMode;
            FillMode = fillMode;
            IsDepthClipEnabled = isDepthClipEnabled;
            IsScissorTestEnabled = isScissorTestEnabled;
        }

        public bool Equals(RasterizerStateCacheKey other)
        {
            return CullMode == other.CullMode
                && FillMode == other.FillMode
                && IsDepthClipEnabled == other.IsDepthClipEnabled
                && IsScissorTestEnabled == other.IsScissorTestEnabled;
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(
                CullMode.GetHashCode(),
                HashHelper.Combine(
                    FillMode.GetHashCode(),
                    HashHelper.Combine(IsDepthClipEnabled.GetHashCode(), IsScissorTestEnabled.GetHashCode())));
        }
    }
}
