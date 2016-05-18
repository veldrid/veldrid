using Newtonsoft.Json;
using System.IO;
using System;

namespace Veldrid.Assets
{
    public class TextAssetLoader<T> : AssetLoader<T>
    {
        public override string FileExtension => "json";

        public override T Load(Stream s)
        {
            JsonSerializer serializer = JsonSerializer.CreateDefault();
            serializer.TypeNameHandling = TypeNameHandling.All;
            using (var sr = new StreamReader(s))
            using (var jtr = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jtr);
            }
        }
    }

    public class ObjectTextAssetLoader : AssetLoader
    {
        public string FileExtension => "json";

        public object Load(Stream s)
        {
            JsonSerializer serializer = JsonSerializer.CreateDefault();
            serializer.TypeNameHandling = TypeNameHandling.All;
            using (var sr = new StreamReader(s))
            using (var jtr = new JsonTextReader(sr))
            {
                return serializer.Deserialize(jtr);
            }
        }
    }
}
