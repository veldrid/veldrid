using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public interface AssetRef<T>
    {
        T Create(RenderContext rc, AssetDatabase ad);
    }
}
