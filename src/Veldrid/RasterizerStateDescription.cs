using System;

namespace Veldrid
{
    public struct RasterizerStateDescription : IEquatable<RasterizerStateDescription>
    {
        public FaceCullMode CullMode;
        public TriangleFillMode FillMode;
        public FrontFace FrontFace;
        public bool DepthClipEnabled;
        public bool ScissorTestEnabled;

        public RasterizerStateDescription(
            FaceCullMode cullMode,
            TriangleFillMode fillMode,
            FrontFace frontFace,
            bool depthClipEnabled,
            bool scissorTestEnabled)
        {
            CullMode = cullMode;
            FillMode = fillMode;
            FrontFace = frontFace;
            DepthClipEnabled = depthClipEnabled;
            ScissorTestEnabled = scissorTestEnabled;
        }

        public static readonly RasterizerStateDescription Default = new RasterizerStateDescription
        {
            CullMode = FaceCullMode.Back,
            FillMode = TriangleFillMode.Solid,
            FrontFace = FrontFace.Clockwise,
            DepthClipEnabled = true,
            ScissorTestEnabled = true,
        };

        public bool Equals(RasterizerStateDescription other)
        {
            return CullMode == other.CullMode
                && FillMode == other.FillMode
                && FrontFace == other.FrontFace
                && DepthClipEnabled.Equals(other.DepthClipEnabled)
                && ScissorTestEnabled.Equals(other.ScissorTestEnabled);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(
                CullMode.GetHashCode(),
                FillMode.GetHashCode(),
                FrontFace.GetHashCode(),
                DepthClipEnabled.GetHashCode(),
                ScissorTestEnabled.GetHashCode());
        }
    }
}
