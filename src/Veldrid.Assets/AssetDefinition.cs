using System.IO;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public interface AssetDefinition
    {
        object Create(AssetDatabase ad);
    }

    public abstract class AssetDefinition<T> : AssetDefinition
    {
        public abstract T Create(AssetDatabase ad);

        object AssetDefinition.Create(AssetDatabase ad)
        {
            return Create(ad);
        }
    }
}
