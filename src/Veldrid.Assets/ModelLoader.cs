using System;
using System.IO;
using System.Linq;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public class ModelLoader : AssetLoader<ObjFile>
    {
        public override string FileExtension => "obj";

        public override ObjFile Load(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                string[] lines = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var objFile = new ObjParser().Parse(lines);
                return objFile;
            }
        }
    }
}
