using Xunit;

namespace Veldrid.Tests
{
    public abstract class FramebufferTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void NoDepthTarget_ClearAllColors_Succeeds()
        {
            Texture colorTarget = RF.CreateTexture(
                new TextureDescription(1024, 1024, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));
            Framebuffer fb = RF.CreateFramebuffer(new FramebufferDescription(null, colorTarget));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetFramebuffer(fb);
            cl.ClearColorTarget(0, RgbaFloat.Red);
            cl.End();
            GD.ExecuteCommands(cl);
            GD.WaitForIdle();

            Texture staging = RF.CreateTexture(
                new TextureDescription(1024, 1024, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));

            cl.Begin();
            cl.CopyTexture(
                colorTarget, 0, 0, 0, 0, 0,
                staging, 0, 0, 0, 0, 0,
                1024, 1024, 1, 1);
            cl.End();
            GD.ExecuteCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaFloat> view = GD.Map<RgbaFloat>(staging, MapMode.Read);
            for (int i = 0; i < view.Count; i++)
            {
                Assert.Equal(RgbaFloat.Red, view[i]);
            }
            GD.Unmap(staging);
        }
    }

    public class VulkanFramebufferTests : FramebufferTests<VulkanDeviceCreator> { }
    public class OpenGLFramebufferTests : FramebufferTests<OpenGLDeviceCreator> { }
#if TEST_D3D11
    public class D3D11FramebufferTests : FramebufferTests<D3D11DeviceCreator> { }
#endif
}
