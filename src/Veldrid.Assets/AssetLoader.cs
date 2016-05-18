using System.IO;

namespace Veldrid.Assets
{
    public interface AssetLoader
    {
        string FileExtension { get; }
        object Load(Stream s);
    }

    public abstract class AssetLoader<TAsset> : AssetLoader
    {
        public abstract string FileExtension { get; }

        public abstract TAsset Load(Stream s);

        object AssetLoader.Load(Stream s) => Load(s);
    }
}
