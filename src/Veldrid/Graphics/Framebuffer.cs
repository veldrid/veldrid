namespace Veldrid.Graphics
{
    public interface Framebuffer : RenderStateModifier
    {
        DeviceTexture ColorTexture { get; }
        DeviceTexture DepthTexture { get; }
    }
}