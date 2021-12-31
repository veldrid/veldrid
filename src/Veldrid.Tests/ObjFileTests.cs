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
            var objFile = new ObjParser().Parse(new string[] { $"mtllib {mtllib}" });
            Assert.Equal(mtllib, objFile.MaterialLibName);
        }
    }
}
