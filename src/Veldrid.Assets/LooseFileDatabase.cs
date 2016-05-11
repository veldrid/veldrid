using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public class LooseFileDatabase : AssetDatabase
    {
        private readonly string _rootPath;
        private Dictionary<Type, AssetLoader> _assetLoaders = new Dictionary<Type, AssetLoader>()
        {
            { typeof(ImageProcessorTexture), new PngLoader() }
        };

        private static readonly JsonSerializer _serializer = CreateDefaultSerializer();

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
            return files.Select(file => Path.ChangeExtension(Path.GetFullPath(file), null)).ToArray();
        }

        public void SaveDefinition<T>(T obj, string name)
        {
            string path = GetAssetPath(name);
            using (var fs = File.CreateText(path))
            {
                _serializer.Serialize(fs, obj);
            }
        }

        public string GetAssetPath(AssetID assetID)
        {
            return Path.Combine(GetAssetBase(), assetID.Value);
        }

        public T LoadAsset<T>(AssetRef<T> definition)
        {
            AssetLoader<T> loader = GetLoader<T>();
            using (var s = OpenAssetStream(definition.ID))
            {
                return loader.Load(s);
            }
        }

        public T LoadAsset<T>(AssetID assetID)
        {
            AssetLoader<T> loader = GetLoader<T>();
            using (var stream = OpenAssetStream(assetID))
            {
                return loader.Load(stream);
            }
        }

        public Stream OpenAssetStream(AssetID assetID)
        {
            string path = GetAssetPath(assetID);
            return File.OpenRead(path);

        }

        private string GetAssetTypeDirectory<T>()
        {
            return Path.Combine(GetAssetBase(), typeof(T).Name);
        }

        private string GetAssetBase()
        {
            return Path.Combine(_rootPath, "Assets");
        }

        private AssetLoader<T> GetLoader<T>()
        {
            AssetLoader ret;
            if (_assetLoaders.TryGetValue(typeof(T), out ret))
            {
                return (AssetLoader<T>)ret;
            }
            else
            {
                return new TextAssetLoader<T>();
            }
        }
    }
}