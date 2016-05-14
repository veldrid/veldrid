using System.IO;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public class ModelLoader : AssetLoader<ObjMeshInfo>
    {
        public override ObjMeshInfo Load(Stream s)
        {
            return ObjImporter.Import(s).Result;
        }
    }
}
