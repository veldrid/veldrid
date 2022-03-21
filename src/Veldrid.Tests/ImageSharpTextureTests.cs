using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
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
            using var image = new Image<Rgba32>(imageSize, imageSize);
            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var flaotValue = x * 1f / image.Width;
                    image[x, y] = new Rgba32(flaotValue, flaotValue, 0, 1);
                }
            }

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
                    var byteValue = (byte)Math.Ceiling(byte.MaxValue * 1f / staging.Width * x);
                    Assert.Equal(new Rgba32(byteValue, byteValue, 0, byte.MaxValue), readView[x, y]);
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
