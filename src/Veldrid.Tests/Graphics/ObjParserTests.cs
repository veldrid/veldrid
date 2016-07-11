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
            string[] lines = File.ReadAllLines(path);
            ObjFile file = new ObjParser().Parse(lines);

            Assert.Equal("sponza.mtl", file.MaterialLibName);
            Assert.Equal(381, file.MeshGroups.Length);
        }

        [Fact]
        public static void ParseTeapot()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Teapot.obj");
            string[] lines = File.ReadAllLines(path);
            ObjFile file = new ObjParser().Parse(lines);

            Assert.Equal(null, file.MaterialLibName);
        }
    }
}
