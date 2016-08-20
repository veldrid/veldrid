using System;

namespace Veldrid.Assets
{
    public interface AssetDatabase
    {
        T LoadAsset<T>(AssetID assetID);
        T LoadAsset<T>(AssetRef<T> assetRef);
        object LoadAsset(AssetID assetID);
        bool TryLoadAsset<T>(AssetID assetID, out T asset);
        AssetID[] GetAssetsOfType(Type t);
    }

    public interface EditableAssetDatabase : AssetDatabase
    {
        string GetAssetPath(AssetID assetID);
        DirectoryNode GetRootDirectoryGraph();
        Type GetAssetType(AssetID assetID);
        void CloneAsset(string path);
        void DeleteAsset(string path);
    }
}
