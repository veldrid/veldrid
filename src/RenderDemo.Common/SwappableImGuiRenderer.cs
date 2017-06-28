using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;
using Veldrid.Platform;

namespace Veldrid.RenderDemo
{
    public class SwappableImGuiRenderer : ImGuiRenderer, SwappableRenderItem
    {
        public SwappableImGuiRenderer(RenderContext rc, Window window) : base(rc, window)
        {
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 pos)
        {
            return new RenderOrderKey();
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            SetRenderContext(rc);
        }
    }
}
