namespace Veldrid.Assets
{
    public interface AssetDatabase
    {
        string[] GetAssetNames<T>();

        T Load<T>(string assetName);

        void Save<T>(T obj, string name);
    }
}
