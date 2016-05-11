using System.IO;

namespace Veldrid.Assets
{
    public interface AssetLoader
    {
        object Load(Stream s);
    }

    public abstract class AssetLoader<TAsset> : AssetLoader
    {
        public abstract TAsset Load(Stream s);

        object AssetLoader.Load(Stream s) => Load(s);
    }
}
