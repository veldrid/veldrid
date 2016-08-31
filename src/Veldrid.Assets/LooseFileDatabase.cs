using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using Veldrid.Graphics;
using System.Diagnostics;
using Veldrid.Assets.Converters;
using System.Reflection;

namespace Veldrid.Assets
{
    public class LooseFileDatabase : EditableAssetDatabase
    {
        private string _rootPath;
        private Dictionary<Type, AssetLoader> _assetLoaders = new Dictionary<Type, AssetLoader>()
        {
            { typeof(ImageProcessorTexture), new PngLoader() },
            { typeof(TextureData), new PngLoader() },
            { typeof(ObjFile), new ObjFileLoader() },
            { typeof(ConstructedMeshInfo), new FirstMeshObjLoader() },
            { typeof(MeshData), new FirstMeshObjLoader() }
        };

        // Used for untyped loads on an asset
        // i.e.: object LoadAsset(AssetID);
        private static readonly Dictionary<string, Type> s_extensionTypeMappings = new Dictionary<string, Type>()
        {
            { ".png", typeof(ImageProcessorTexture) },
            { ".jpg", typeof(ImageProcessorTexture) },
            { ".obj", typeof(ConstructedMeshInfo) }
        };

        private Dictionary<AssetID, object> _loadedAssets = new Dictionary<AssetID, object>();

        private static readonly JsonSerializer _serializer = CreateDefaultSerializer();

        public JsonSerializer DefaultSerializer => _serializer;

        public string RootPath { get { return _rootPath; } set { _rootPath = value; } }

        public LooseFileDatabase(string rootPath)
        {
            _rootPath = rootPath;
        }

        public static void AddExtensionTypeMapping(string extension, Type type)
        {
            s_extensionTypeMappings.Add(extension, type);
        }

        private static JsonSerializer CreateDefaultSerializer()
        {
            JsonConverter[] converters = new JsonConverter[]
            {
                new VertexPositionNormalTextureConverter(),
                new RgbaFloatConverter()
            };

            return JsonSerializer.Create(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                Converters = converters
            });
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

        public T LoadAsset<T>(AssetRef<T> definition) => LoadAsset<T>(definition.ID, true);
        public T LoadAsset<T>(AssetID assetID) => LoadAsset<T>(assetID, true);

        public T LoadAsset<T>(AssetRef<T> definition, bool cache = true) => LoadAsset<T>(definition.ID, cache);
        public T LoadAsset<T>(AssetID assetID, bool cache  = true)
        {
            object asset;
            if (!cache || !_loadedAssets.TryGetValue(assetID, out asset))
            {
                AssetLoader<T> loader = GetLoader<T>();
                using (var s = OpenAssetStream(assetID))
                {
                    asset = loader.Load(s);
                    if (cache)
                    {
                        _loadedAssets.Add(assetID, asset);
                    }
                }
            }

            return (T)asset;
        }

        public object LoadAsset(AssetID id)
        {
            Type t = GetAssetType(id);
            return LoadAsset(t, id);
        }

        public object LoadAsset(Type t, AssetID id)
        {
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
                var asset = LoadAsset(typeof(object), id);
                type = asset.GetType();
                s_extensionTypeMappings.Add(extension, type);
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

        public AssetID[] GetAssetsOfType(Type t)
        {
            List<AssetID> discovered = new List<AssetID>();
            HashSet<string> extensions = new HashSet<string>();
            foreach (var kvp in s_extensionTypeMappings)
            {
                if (t.GetTypeInfo().IsAssignableFrom(kvp.Value))
                {
                    extensions.Add(kvp.Key);
                }
            }

            var node = GetRootDirectoryGraph();
            DiscoverAssets(node, extensions, discovered);

            return discovered.ToArray();
        }

        private void DiscoverAssets(DirectoryNode node, HashSet<string> extensions, List<AssetID> discovered)
        {
            foreach (var asset in node.AssetInfos)
            {
                if (extensions.Contains(Path.GetExtension(asset.Path)))
                {
                    string fullPath = asset.Path;
                    AssetID id = GetIdFromFullPath(fullPath);
                    discovered.Add(id);
                }
            }

            foreach (var child in node.Children)
            {
                DiscoverAssets(child, extensions, discovered);
            }
        }

        private AssetID GetIdFromFullPath(string fullPath)
        {
            Debug.Assert(fullPath.StartsWith(_rootPath));
            return fullPath.Substring(_rootPath.Length + 1, fullPath.Length - _rootPath.Length - 1);
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
                return new TextAssetLoader<T>(_serializer);
            }
        }

        private AssetLoader GetLoader(Type t)
        {
            AssetLoader loader;
            if (!_assetLoaders.TryGetValue(t, out loader))
            {
                loader = new TextAssetLoader<object>(_serializer);
                _assetLoaders.Add(t, loader);
            }

            return loader;
        }

        public bool TryLoadAsset<T>(AssetID id, out T asset)
        {
            if (File.Exists(GetAssetPath(id)))
            {
                asset = LoadAsset<T>(id);
                return true;
            }
            else
            {
                asset = default(T);
                return false;
            }
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
