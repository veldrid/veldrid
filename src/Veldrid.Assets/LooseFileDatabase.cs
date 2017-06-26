using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using Veldrid.Graphics;
using System.Diagnostics;
using Veldrid.Assets.Converters;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;

namespace Veldrid.Assets
{
    public class LooseFileDatabase : EditableAssetDatabase
    {
        private string _rootPath;
        private Dictionary<Type, AssetLoader> _assetLoaders = new Dictionary<Type, AssetLoader>()
        {
            { typeof(ImageSharpMipmapChain), new PngLoader() },
            { typeof(ImageSharpTexture), new ImageSharpTextureLoader() },
            { typeof(TextureData), new PngLoader() },
            { typeof(ObjFile), new ObjFileLoader() },
            { typeof(ConstructedMeshInfo), new FirstMeshObjLoader() },
            { typeof(MeshData), new FirstMeshObjLoader() }
        };

        // Used for untyped loads on an asset
        // i.e.: object LoadAsset(AssetID);
        private static readonly Dictionary<string, Type> s_extensionTypeMappings = new Dictionary<string, Type>()
        {
            { ".png", typeof(ImageSharpTexture) },
            { ".jpg", typeof(ImageSharpTexture) },
            { ".obj", typeof(ConstructedMeshInfo) }
        };

        private ConcurrentDictionary<AssetID, object> _loadedAssets = new ConcurrentDictionary<AssetID, object>();

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

        public void RegisterTypeLoader(Type t, AssetLoader loader)
        {
            _assetLoaders.Add(t, loader);
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

        public string SaveDefinition<T>(T obj, string name)
        {
            string path = GetAssetPath(name);
            using (var fs = File.CreateText(path))
            {
                _serializer.Serialize(fs, obj);
            }

            AssetID id = new AssetID(name);
            _loadedAssets[id] = obj;
            return path;
        }

        public override string GetAssetPath(AssetID assetID)
        {
            return Path.Combine(_rootPath, assetID.Value);
        }

        public override T LoadAsset<T>(AssetRef<T> definition, bool cache) => LoadAsset<T>(definition.ID, cache);
        public override T LoadAsset<T>(AssetID assetID, bool cache)
        {
            object asset;
            if (cache)
            {
                asset = _loadedAssets.GetOrAdd(assetID, id =>
                {
                    AssetLoader loader = GetLoader<T>();
                    using (var s = OpenAssetStream(assetID))
                    {
                        return loader.Load(s);
                    }
                });
            }
            else
            {
                AssetLoader loader = GetLoader<T>();
                using (var s = OpenAssetStream(assetID))
                {
                    asset = loader.Load(s);
                }
            }

            return (T)asset;
        }

        public override object LoadAsset(AssetID id, bool cache)
        {
            Type t = GetAssetType(id);
            return LoadAsset(t, id, cache);
        }

        private object LoadAsset(Type t, AssetID id, bool cache)
        {
            // TODO: Cache stuff here.
            AssetLoader loader = GetLoader(t);
            using (var stream = OpenAssetStream(id))
            {
                return loader.Load(stream);
            }
        }

        public override Type GetAssetType(AssetID id)
        {
            string extension = Path.GetExtension(id.Value);
            Type type;
            if (!s_extensionTypeMappings.TryGetValue(extension, out type))
            {
                var asset = LoadAsset(typeof(object), id, true);
                type = asset.GetType();
                s_extensionTypeMappings.Add(extension, type);
            }

            return type;
        }

        public override void CloneAsset(string path)
        {
            string folder = new FileInfo(path).Directory.FullName;
            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            string destination = Path.Combine(folder, fileName + "_Copy." + extension);
            File.Copy(path, destination);
        }

        public override void DeleteAsset(string path)
        {
            File.Delete(path);
        }

        public override bool TryOpenAssetStream(AssetID assetID, out Stream stream)
        {
            string path = GetAssetPath(assetID);
            if (File.Exists(path))
            {
                stream = File.OpenRead(path);
                return true;
            }

            stream = null;
            return false;
        }

        public override Stream OpenAssetStream(AssetID assetID)
        {
            string path = GetAssetPath(assetID);
            return File.OpenRead(path);
        }

        public override DirectoryNode GetRootDirectoryGraph() => GetDirectoryGraph(_rootPath);
        private static DirectoryNode GetDirectoryGraph(string path)
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(path);
            if (rootDirectory.Exists)
            {
                var assetInfos = rootDirectory.EnumerateFiles().Select(fi => new AssetInfo(fi.Name, fi.FullName)).ToArray();
                var children = rootDirectory.EnumerateDirectories().Select(di => GetDirectoryGraph(di.FullName)).ToArray();

                return new DirectoryNode(path, assetInfos.ToArray(), children);
            }
            return new DirectoryNode(path, Array.Empty<AssetInfo>(), Array.Empty<DirectoryNode>());
        }

        public override AssetID[] GetAssetsOfType(Type t)
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

        private AssetLoader GetLoader<T>()
        {
            return GetLoader(typeof(T));
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

        public override bool TryLoadAsset<T>(AssetID id, bool cache, out T asset)
        {
            if (File.Exists(GetAssetPath(id)))
            {
                asset = LoadAsset<T>(id, cache);
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
