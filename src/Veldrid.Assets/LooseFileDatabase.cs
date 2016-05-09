using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Veldrid.Assets
{
    public class LooseFileDatabase : AssetDatabase
    {
        private static readonly JsonSerializer _serializer = CreateDefaultSerializer();
        private readonly string _rootPath;

        public LooseFileDatabase(string rootPath)
        {
            _rootPath = rootPath;
        }

        private static JsonSerializer CreateDefaultSerializer()
        {
            return new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public string[] GetAssetNames<T>()
        {
            string path = GetAssetTypeDirectory<T>();
            var files = Directory.EnumerateFiles(path);
            return files.Select(file => Path.GetFileNameWithoutExtension(file)).ToArray();
        }

        public T Load<T>(string assetName)
        {
            string path = GetAssetPath<T>(assetName);
            using (var fs = File.OpenText(path))
            {
                return _serializer.Deserialize<T>(new JsonTextReader(fs));
            }
        }

        public void Save<T>(T obj, string name)
        {
            string path = GetAssetPath<T>(name);
            using (var fs = File.CreateText(path))
            {
                _serializer.Serialize(fs, obj);
            }
        }

        public string GetAssetPath<T>(string assetName)
        {
            string typeName = typeof(T).Name;
            return Path.Combine(GetAssetTypeDirectory<T>(), assetName + ".json");
        }

        private string GetAssetTypeDirectory<T>()
        {
            return Path.Combine(GetAssetBase(), typeof(T).Name);
        }

        private string GetAssetBase()
        {
            return Path.Combine(_rootPath, "Assets");
        }
    }
}