using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Veldrid.Assets;

namespace Veldrid.RenderDemo
{
    internal static class AssetDatabase
    {
        private static readonly JsonSerializer _serializer = CreateDefaultSerializer();

        private static JsonSerializer CreateDefaultSerializer()
        {
            return new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public static string[] GetAssetNames<T>()
        {
            string path = GetAssetTypeDirectory<T>();
            var files = Directory.EnumerateFiles(path);
            return files.Select(file => Path.GetFileNameWithoutExtension(file)).ToArray();
        }

        public static T Load<T>(string assetName)
        {
            string path = GetAssetPath<T>(assetName);
            using (var fs = File.OpenText(path))
            {
                return _serializer.Deserialize<T>(new JsonTextReader(fs));
            }
        }

        public static void Save<T>(T obj, string name)
        {
            string path = GetAssetPath<T>(name);
            using (var fs = File.CreateText(path))
            {
                _serializer.Serialize(fs, obj);
            }
        }

        public static string GetAssetPath<T>(string assetName)
        {
            string typeName = typeof(T).Name;
            return Path.Combine(GetAssetTypeDirectory<T>(), assetName + ".json");
        }

        private static string GetAssetTypeDirectory<T>()
        {
            return Path.Combine(GetAssetBase(), typeof(T).Name);
        }

        private static string GetAssetBase()
        {
            return Path.Combine(AppContext.BaseDirectory, "Assets");
        }
    }
}