using System;
using System.IO;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public class ModelLoader : AssetLoader<ObjMeshInfo>
    {
        public override string FileExtension => "obj";

        public override ObjMeshInfo Load(Stream s)
        {
            return ObjImporter.Import(s).Result;
        }
    }
}
