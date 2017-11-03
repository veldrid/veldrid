using System;

namespace Veldrid
{
    public struct FramebufferDescription : IEquatable<FramebufferDescription>
    {
        public Texture DepthTarget;
        public Texture[] ColorTargets;

        public FramebufferDescription(Texture depthTarget, params Texture[] colorTargets)
        {
            DepthTarget = depthTarget;
            ColorTargets = colorTargets;
        }

        public bool Equals(FramebufferDescription other)
        {
            return DepthTarget.Equals(other.DepthTarget) && Util.ArrayEquals(ColorTargets, other.ColorTargets);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(DepthTarget.GetHashCode(), HashHelper.Array(ColorTargets));
        }
    }
}
