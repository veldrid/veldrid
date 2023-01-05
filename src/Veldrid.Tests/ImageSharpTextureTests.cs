using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid.ImageSharp;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class ImageSharpTextureTestsTestBase<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void CreateDeviceResource_ThenRead()
        {
            const int imageSize = 10;
            var pixels = new Rgba32[imageSize * imageSize];
            for (byte x = 0; x < imageSize; x++)
            {
                for (byte y = 0; y < imageSize; y++)
                {
                    pixels[x + y * imageSize] = new Rgba32(x, y, 0, byte.MaxValue);
                }
            }
            using var image = Image.LoadPixelData(pixels, imageSize, imageSize);

            var texture = new ImageSharpTexture(image, mipmap: false);
            using var deviceTexture = texture.CreateDeviceTexture(GD, RF);

            Assert.Equal(1u, deviceTexture.ArrayLayers);
            Assert.Equal(1u, deviceTexture.MipLevels);
            Assert.Equal((uint)imageSize, deviceTexture.Width);
            Assert.Equal((uint)imageSize, deviceTexture.Height);
            using var staging = GetReadback(deviceTexture);
            var readView = GD.Map<Rgba32>(staging, MapMode.Read);
            for (var x = 0; x < staging.Width; x++)
            { 
                for (var y = 0; y < staging.Height; y++)
                {
                    Assert.Equal(pixels[x + y * imageSize], readView[x, y]);
                }
            }
            GD.Unmap(staging);
        }
    }


#if TEST_VULKAN
    [Trait("Backend", "Vulkan")]
    public class VulkanImageSharpTextureTests : ImageSharpTextureTestsTestBase<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    [Trait("Backend", "D3D11")]
    public class D3D11ImageSharpTextureTests : ImageSharpTextureTestsTestBase<D3D11DeviceCreator> { }
#endif
#if TEST_METAL
    [Trait("Backend", "Metal")]
    public class MetalTextureTests : ImageSharpTextureTestsTestBase<MetalDeviceCreator> { }
#endif
#if TEST_OPENGL
    [Trait("Backend", "OpenGL")]
    public class OpenGLImageSharpTextureTests : ImageSharpTextureTestsTestBase<OpenGLDeviceCreator> { }
#endif
#if TEST_OPENGLES
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESImageSharpTextureTests : ImageSharpTextureTestsTestBase<OpenGLESDeviceCreator> { }
#endif
}
