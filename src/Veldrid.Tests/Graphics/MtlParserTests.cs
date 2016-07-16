using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Veldrid.Graphics
{
    public static class MtlParserTests
    {
        [Fact]
        public static void ParseSponzaAtriumMaterials()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "sponza.mtl");
            string[] lines = File.ReadAllLines(path);
            MtlParser parser = new MtlParser();
            MtlFile file = parser.Parse(lines);

            Assert.Equal(26, file.Definitions.Values.Count());
            Assert.Equal("textures\\sponza_roof_diff.png", file.Definitions["roof"].AmbientTexture);
            Assert.Equal("textures\\sponza_roof_diff.png", file.Definitions["roof"].DiffuseTexture);

            Assert.Equal("textures\\sponza_curtain_blue_diff.png", file.Definitions["fabric_g"].AmbientTexture);
            Assert.Equal("textures\\sponza_curtain_blue_diff.png", file.Definitions["fabric_g"].DiffuseTexture);
        }
    }
}
