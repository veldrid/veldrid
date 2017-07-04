using System;
using System.IO;
using Xunit;

namespace Veldrid.Graphics
{
    public static class ObjParserTests
    {
        [Fact]
        public static void ParseSponzaAtriumFile()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "sponza.obj");
            string text = File.ReadAllText(path);
            ObjFile file = new ObjParser().Parse(text);

            Assert.Equal("sponza.mtl", file.MaterialLibName);
            Assert.Equal(393, file.MeshGroups.Length);
        }

        [Fact]
        public static void ParseTeapot()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Teapot.obj");
            string lines = File.ReadAllText(path);
            ObjFile file = new ObjParser().Parse(lines);

            Assert.Equal(null, file.MaterialLibName);
        }
    }
}
