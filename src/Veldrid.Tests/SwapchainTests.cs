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

#if TEST_VULKAN
    public class VulkanSwapchainTests : SwapchainTests<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    public class D3D11SwapchainTests : SwapchainTests<D3D11DeviceCreator> { }
#endif
#if TEST_METAL
    public class MetalSwapchainTests : SwapchainTests<MetalDeviceCreator> { }
#endif
}
