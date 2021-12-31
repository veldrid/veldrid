using Veldrid.Utilities;
using Xunit;

namespace Veldrid.Tests
{
    public class ObjFileTests
    {
        [Theory]
        [InlineData("space_station.mtl")]
        [InlineData("space station.mtl")]
        public void CanParseMtllib(string mtllib)
        {
            var content = new string[] { $"mtllib {mtllib}" };
            var objFile = ObjFile.Parse(content);
            Assert.Equal(mtllib, objFile.MaterialLibName);
        }
    }
}
