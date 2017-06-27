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
                string text = sr.ReadToEnd();
                var objFile = new ObjParser().Parse(text);
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
                string text = sr.ReadToEnd();
                var objFile = new ObjParser().Parse(text);
                return objFile.GetFirstMesh();
            }
        }
    }
}
