using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public interface SwappableRenderItem : RenderItem
    {
        void ChangeRenderContext(RenderContext rc);
    }
}
