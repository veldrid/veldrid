using Xunit;

namespace Veldrid.Tests
{
    public abstract class TextureTestBase<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void Map_Succeeds()
        {
            Texture texture = RF.CreateTexture(
                new TextureDescription(1024, 1024, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));

            MappedResource map = GD.Map(texture, MapMode.ReadWrite, 0);
            GD.Unmap(texture, 0);
        }
    }

    public class VulkanTextureTests : TextureTestBase<VulkanDeviceCreator> { }
    public class D3D11TextureTests : TextureTestBase<D3D11DeviceCreator> { }
    public class OpenGLTextureTests : TextureTestBase<OpenGLDeviceCreator> { }
}
