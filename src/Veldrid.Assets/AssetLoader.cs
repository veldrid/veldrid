using Newtonsoft.Json;
using System;
using System.IO;

namespace Veldrid.Assets
{
    internal interface AssetLoader
    {
        Type TypeLoaded { get; }
        object Load(Stream s, string name);
    }

    internal interface AssetLoader<TRet, TAsset> : AssetLoader where TAsset : AssetRef<TRet>
    {
        new TRet Load(Stream s, string name);
    }

    public class GenericAssetLoader
    {
        private static readonly JsonSerializer s_defaultSerializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.Auto };

        public object Load(Stream s, string name, Type t)
        {
            using (var sr = new StreamReader(s))
            {
                return s_defaultSerializer.Deserialize(sr, t);
            }
        }
    }
}
