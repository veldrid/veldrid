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
            using var image = new Image<Rgba32>(10, 10);
            for(var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var flaotValue = x * 1f / image.Width;
                    image[x, y] = new Rgba32(flaotValue, flaotValue, 0, 1);
                }
            }

            var texture = new ImageSharpTexture(image, mipmap: false);
            using var deviceTexture = texture.CreateDeviceTexture(GD, RF);


            var staging = RF.CreateTexture(TextureDescription.Texture2D(
                (uint)image.Width, (uint) image.Height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            var cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(
                deviceTexture, 0, 0, 0, 0, 0,
                staging, 0, 0, 0, 0, 0,
                (uint) image.Width, (uint)image.Height, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            for (uint layer = 0; layer < staging.ArrayLayers; layer++)
            {
                MappedResourceView<RgbaByte> readView = GD.Map<RgbaByte>(staging, MapMode.Read, layer);
                for (int x = 0; x < staging.Width; x++)
                { 
                    for (int y = 0; y < staging.Height; y++)
                    {
                        var byteValue = (byte)Math.Ceiling(byte.MaxValue * 1f / staging.Width * x);
                        Assert.Equal(new RgbaByte(byteValue, byteValue, (byte)layer, 255), readView[x, y]);
                    }
                }
                GD.Unmap(staging, layer);
            }
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
