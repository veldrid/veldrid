namespace Vd2
{
    public struct FramebufferDescription
    {
        public Texture2D DepthTarget;
        public Texture2D[] ColorTargets;

        public FramebufferDescription(Texture2D depthTarget, params Texture2D[] colorTargets)
        {
            DepthTarget = depthTarget;
            ColorTargets = colorTargets;
        }
    }
}
