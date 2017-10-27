using System;

namespace Veldrid
{
    public struct FramebufferDescription : IEquatable<FramebufferDescription>
    {
        public Texture2D DepthTarget;
        public Texture2D[] ColorTargets;

        public FramebufferDescription(Texture2D depthTarget, params Texture2D[] colorTargets)
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
