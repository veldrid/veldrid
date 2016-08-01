using System;
using System.IO;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public class ObjFileLoader : ConcreteLoader<ObjFile>
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

    public class FirstMeshObjLoader : ConcreteLoader<ConstructedMeshInfo>
    {
        public override string FileExtension => "obj";

        public override ConstructedMeshInfo Load(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                string[] lines = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var objFile = new ObjParser().Parse(lines);
                return objFile.GetFirstMesh();
            }
        }
    }
}
