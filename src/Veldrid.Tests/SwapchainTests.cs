using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class SwapchainTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Theory]
        [InlineData(PixelFormat.R16_UNorm, false)]
        [InlineData(PixelFormat.R16_UNorm, true)]
        [InlineData(PixelFormat.R32_Float, false)]
        [InlineData(PixelFormat.R32_Float, true)]
        [InlineData(null, false)]
        [InlineData(null, true)]
        public void Ctor_SetsProperties(PixelFormat? depthFormat, bool syncToVerticalBlank)
        {
            Sdl2Window window = new Sdl2Window("SwapchainTestWindow", 0, 0, 100, 100, SDL_WindowFlags.Hidden, false);
            SwapchainSource source = VeldridStartup.GetSwapchainSource(window);
            SwapchainDescription swapchainDesc = new SwapchainDescription(source, 100, 100, depthFormat, syncToVerticalBlank);
            Swapchain swapchain = RF.CreateSwapchain(ref swapchainDesc);

            if (depthFormat == null)
            {
                Assert.Null(swapchain.Framebuffer.DepthTarget);
            }
            else
            {
                Assert.NotNull(swapchain.Framebuffer.DepthTarget);
                Assert.Equal(depthFormat, swapchain.Framebuffer.DepthTarget.Value.Target.Format);
            }

            Assert.Equal(syncToVerticalBlank, swapchain.SyncToVerticalBlank);

            window.Close();
        }
    }

    public abstract class MainSwapchainTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void Textures_Properties_Correct()
        {
            Texture colorTarget = GD.MainSwapchain.Framebuffer.ColorTargets[0].Target;
            Assert.Equal(TextureType.Texture2D, colorTarget.Type);
            Assert.InRange(colorTarget.Width, 1u, uint.MaxValue);
            Assert.InRange(colorTarget.Height, 1u, uint.MaxValue);
            Assert.Equal(1u, colorTarget.Depth);
            Assert.Equal(1u, colorTarget.ArrayLayers);
            Assert.Equal(1u, colorTarget.MipLevels);
            Assert.Equal(TextureUsage.RenderTarget, colorTarget.Usage);
            Assert.Equal(TextureSampleCount.Count1, colorTarget.SampleCount);

            Texture depthTarget = GD.MainSwapchain.Framebuffer.DepthTarget.Value.Target;
            Assert.Equal(TextureType.Texture2D, depthTarget.Type);
            Assert.Equal(colorTarget.Width, depthTarget.Width);
            Assert.Equal(colorTarget.Height, depthTarget.Height);
            Assert.Equal(1u, depthTarget.Depth);
            Assert.Equal(1u, depthTarget.ArrayLayers);
            Assert.Equal(1u, depthTarget.MipLevels);
            Assert.Equal(TextureUsage.DepthStencil, depthTarget.Usage);
            Assert.Equal(TextureSampleCount.Count1, depthTarget.SampleCount);
        }
    }

#if TEST_VULKAN
    [Trait("Backend", "Vulkan")]
    public class VulkanSwapchainTests : SwapchainTests<VulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanMainSwapchainTests : MainSwapchainTests<VulkanDeviceCreatorWithMainSwapchain> { }
#endif
#if TEST_D3D11
    [Trait("Backend", "D3D11")]
    public class D3D11SwapchainTests : SwapchainTests<D3D11DeviceCreator> { }
    [Trait("Backend", "D3D11")]
    public class D3D11MainSwapchainTests : MainSwapchainTests<D3D11DeviceCreatorWithMainSwapchain> { }
#endif
#if TEST_METAL
    [Trait("Backend", "Metal")]
    public class MetalSwapchainTests : SwapchainTests<MetalDeviceCreator> { }
    [Trait("Backend", "Metal")]
    public class MetalMainSwapchainTests : SwapchainTests<MetalDeviceCreatorWithMainSwapchain> { }
#endif
#if TEST_OPENGL
    [Trait("Backend", "OpenGL")]
    public class OpenGLMainSwapchainTests : MainSwapchainTests<OpenGLDeviceCreator> { }
#endif
}
