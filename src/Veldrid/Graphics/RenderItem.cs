namespace Veldrid.Graphics
{
    public interface RenderItem
    {
        void Render(RenderContext context);
        RenderOrderKey GetRenderOrderKey();
        void ChangeRenderContext(RenderContext context);
    }
}