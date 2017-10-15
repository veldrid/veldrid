namespace Vd2
{
    public struct RenderPassDescription
    {
        public Framebuffer Framebuffer;
        public RgbaFloat[] ColorClears;
        public float DepthClear;
    }
}