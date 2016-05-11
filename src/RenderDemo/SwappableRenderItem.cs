using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public interface SwappableRenderItem : RenderItem
    {
        void ChangeRenderContext(AssetDatabase ad, RenderContext rc);
    }
}
