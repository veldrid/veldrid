using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using Veldrid.Graphics;
using System.Diagnostics;

namespace Veldrid.Assets
{
    public class LooseFileDatabase : AssetDatabase
    {
        private readonly string _rootPath;
        private Dictionary<Type, AssetLoader> _assetLoaders = new Dictionary<Type, AssetLoader>()
        {
            { typeof(ImageProcessorTexture), new PngLoader() },
            { typeof(ObjMeshInfo), new ModelLoader() }
        };

        private static readonly Dictionary<string, Type> s_extensionTypeMappings = new Dictionary<string, Type>()
        {
            { ".png", typeof(ImageProcessorTexture) },
            { ".obj", typeof(ObjMeshInfo) }
        };

        private Dictionary<AssetID, object> _loadedAssets = new Dictionary<AssetID, object>();

        private static readonly JsonSerializer _serializer = CreateDefaultSerializer();

        public LooseFileDatabase(string rootPath)
        {
            _rootPath = rootPath;
        }

        private static JsonSerializer CreateDefaultSerializer()
        {
            return new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public void SaveDefinition<T>(T obj, string name)
        {
            string path = GetAssetPath(name);
            using (var fs = File.CreateText(path))
            {
                _serializer.Serialize(fs, obj);
            }

            _loadedAssets[new AssetID(name)] = obj;
        }

        public string GetAssetPath(AssetID assetID)
        {
            return Path.Combine(_rootPath, assetID.Value);
        }

        public T LoadAsset<T>(AssetRef<T> definition) => LoadAsset<T>(definition.ID);
        public T LoadAsset<T>(AssetID assetID)
        {
            object asset;
            if (!_loadedAssets.TryGetValue(assetID, out asset))
            {
                AssetLoader<T> loader = GetLoader<T>();
                using (var s = OpenAssetStream(assetID))
                {
                    asset = loader.Load(s);
                }
            }

            return (T)asset;
        }

        public object LoadAsset(AssetID id)
        {
            Type t = GetAssetType(id);
            AssetLoader loader = GetLoader(t);
            using (var stream = OpenAssetStream(id))
            {
                return loader.Load(stream);
            }
        }

        public Type GetAssetType(AssetID id)
        {
            string extension = Path.GetExtension(id.Value);
            Type type;
            if (!s_extensionTypeMappings.TryGetValue(extension, out type))
            {
                type = typeof(object);
            }

            return type;
        }

        public void CloneAsset(string path)
        {
            string folder = new FileInfo(path).Directory.FullName;
            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            string destination = Path.Combine(folder, fileName + "_Copy." + extension);
            File.Copy(path, destination);
        }

        public void DeleteAsset(string path)
        {
            File.Delete(path);
        }

        public Stream OpenAssetStream(AssetID assetID)
        {
            string path = GetAssetPath(assetID);
            return File.OpenRead(path);

        }

        public DirectoryNode GetRootDirectoryGraph() => GetDirectoryGraph(_rootPath);
        private static DirectoryNode GetDirectoryGraph(string path)
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(path);

            var assetInfos = rootDirectory.EnumerateFiles().Select(fi => new AssetInfo(fi.Name, fi.FullName)).ToArray();
            var children = rootDirectory.EnumerateDirectories().Select(di => GetDirectoryGraph(di.FullName)).ToArray();

            return new DirectoryNode(path, assetInfos.ToArray(), children);
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

        private AssetLoader GetLoader(Type t)
        {
            AssetLoader loader;
            if (!_assetLoaders.TryGetValue(t, out loader))
            {
                Debug.Assert(t == typeof(object));
                loader = new ObjectTextAssetLoader();
                _assetLoaders.Add(typeof(object), loader);
            }

            return loader;
        }
    }

    public class AssetInfo
    {
        public string Name { get; }
        public string Path { get; }

        public AssetInfo(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }

    public class DirectoryNode
    {
        public string FullPath { get; }
        public string FolderName { get; set; }
        public AssetInfo[] AssetInfos { get; }
        public DirectoryNode[] Children { get; }

        public DirectoryNode(string path, AssetInfo[] assetInfos, DirectoryNode[] children)
        {
            FullPath = path;
            FolderName = new DirectoryInfo(path).Name;
            AssetInfos = assetInfos;
            Children = children;
        }
    }
}
