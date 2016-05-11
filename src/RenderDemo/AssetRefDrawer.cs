using Veldrid.Assets;

namespace Veldrid.RenderDemo
{
    public class AssetRefDrawer<T> : Drawer<AssetRef<T>>
    {
        public override bool Draw(string label, ref AssetRef<T> obj)
        {
            object assetName = obj.ID.Value;
            if (DrawerCache.GetDrawer(typeof(string)).Draw(label, ref assetName))
            {
                obj = new AssetRef<T>((string)assetName);
                return true;
            }

            return false;
        }
    }
}
