using Newtonsoft.Json;
using System.IO;

namespace Veldrid.Assets
{
    public class TextAssetLoader<T> : AssetLoader<T>
    {
        public override T Load(Stream s)
        {
            JsonSerializer serializer = JsonSerializer.CreateDefault();
            serializer.TypeNameHandling = TypeNameHandling.Auto;
            using (var sr = new StreamReader(s))
            using (var jtr = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jtr);
            }
        }
    }
}
