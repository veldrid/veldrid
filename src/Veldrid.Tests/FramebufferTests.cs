using System;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class FramebufferTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void NoDepthTarget_ClearAllColors_Succeeds()
        {
            Texture colorTarget = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));
            Framebuffer fb = RF.CreateFramebuffer(new FramebufferDescription(null, colorTarget));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetFramebuffer(fb);
            cl.ClearColorTarget(0, RgbaFloat.Red);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            Texture staging = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));

            cl.Begin();
            cl.CopyTexture(
                colorTarget, 0, 0, 0, 0, 0,
                staging, 0, 0, 0, 0, 0,
                1024, 1024, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaFloat> view = GD.Map<RgbaFloat>(staging, MapMode.Read);
            for (int i = 0; i < view.Count; i++)
            {
                Assert.Equal(RgbaFloat.Red, view[i]);
            }
            GD.Unmap(staging);
        }

        [Fact]
        public void NoDepthTarget_ClearDepth_Fails()
        {
            Texture colorTarget = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));
            Framebuffer fb = RF.CreateFramebuffer(new FramebufferDescription(null, colorTarget));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetFramebuffer(fb);
            Assert.Throws<VeldridException>(() => cl.ClearDepthStencil(1f));
        }

        [Fact]
        public void NoColorTarget_ClearColor_Fails()
        {
            Texture depthTarget = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 1, 1, PixelFormat.R16_UNorm, TextureUsage.DepthStencil));
            Framebuffer fb = RF.CreateFramebuffer(new FramebufferDescription(depthTarget));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetFramebuffer(fb);
            Assert.Throws<VeldridException>(() => cl.ClearColorTarget(0, RgbaFloat.Red));
        }

        [Fact]
        public void ClearColorTarget_OutOfRange_Fails()
        {
            TextureDescription desc = TextureDescription.Texture2D(
                1024, 1024, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget);
            Texture colorTarget0 = RF.CreateTexture(desc);
            Texture colorTarget1 = RF.CreateTexture(desc);
            Framebuffer fb = RF.CreateFramebuffer(new FramebufferDescription(null, colorTarget0, colorTarget1));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetFramebuffer(fb);
            cl.ClearColorTarget(0, RgbaFloat.Red);
            cl.ClearColorTarget(1, RgbaFloat.Red);
            Assert.Throws<VeldridException>(() => cl.ClearColorTarget(2, RgbaFloat.Red));
            Assert.Throws<VeldridException>(() => cl.ClearColorTarget(3, RgbaFloat.Red));
        }

        [Fact]
        public void NonZeroMipLevel_ClearColor_Succeeds()
        {
            Texture testTex = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 11, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));

            Framebuffer[] framebuffers = new Framebuffer[11];
            for (uint level = 0; level < 11; level++)
            {
                framebuffers[level] = RF.CreateFramebuffer(
                    new FramebufferDescription(null, new[] { new FramebufferAttachmentDescription(testTex, 0, level) }));
            }

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            for (uint level = 0; level < 11; level++)
            {
                cl.SetFramebuffer(framebuffers[level]);
                cl.ClearColorTarget(0, new RgbaFloat(level, level, level, 1));
            }
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            Texture readback = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 11, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));
            cl.Begin();
            cl.CopyTexture(testTex, readback);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            uint mipWidth = 1024;
            uint mipHeight = 1024;
            for (uint level = 0; level < 11; level++)
            {
                MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(readback, MapMode.Read, level);
                for (uint y = 0; y < mipHeight; y++)
                    for (uint x = 0; x < mipWidth; x++)
                    {
                        Assert.Equal(new RgbaFloat(level, level, level, 1), readView[x, y]);
                    }

                GD.Unmap(readback, level);
                mipWidth = Math.Max(1, mipWidth / 2);
                mipHeight = Math.Max(1, mipHeight / 2);
            }
        }
    }

    public abstract class SwapchainFramebufferTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void ClearSwapchainFramebuffer_Succeeds()
        {
            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetFramebuffer(GD.SwapchainFramebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Red);
            cl.ClearDepthStencil(1f);
            cl.End();
        }

        [Fact]
        public void ReadbackSwapchainFramebuffer_Succeedes()
        {
            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetFramebuffer(GD.SwapchainFramebuffer);
            cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            Texture colorTarget = GD.SwapchainFramebuffer.ColorTargets[0].Target;
            Texture readback = GetReadback(colorTarget);

            cl.Begin();
            cl.SetFramebuffer(GD.SwapchainFramebuffer);
            cl.CopyTexture(colorTarget, readback);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            BgraByte clearColor = new BgraByte(RgbaByte.CornflowerBlue);
            MappedResourceView<BgraByte> readView = GD.Map<BgraByte>(readback, MapMode.Read);
            (uint w, uint h) = (colorTarget.Width, colorTarget.Height);
            for (uint y = 0; y < h; y++)
            for (uint x = 0; x < w; x++)
            {
                Assert.Equal(clearColor, readView[x, y]);
            }
            GD.Unmap(readback);
        }

        [Fact]
        public void ReadbackSwapchainFramebuffer_WhenInactive()
        {
            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetFramebuffer(GD.SwapchainFramebuffer);
            cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            Texture colorTarget = GD.SwapchainFramebuffer.ColorTargets[0].Target;
            (uint w, uint h) = (colorTarget.Width, colorTarget.Height);
            Texture secondTarget = RF.CreateTexture(TextureDescription.Texture2D(
                w, h, 1, 1, colorTarget.Format, TextureUsage.RenderTarget));
            Framebuffer secondFramebuffer = RF.CreateFramebuffer(new FramebufferDescription(null, secondTarget));

            Texture readback = RF.CreateTexture(TextureDescription.Texture2D(
                w, h, 1, 1, colorTarget.Format, TextureUsage.Staging));

            cl.Begin();
            cl.SetFramebuffer(secondFramebuffer);
            cl.CopyTexture(colorTarget, readback);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            BgraByte clearColor = new BgraByte(RgbaByte.CornflowerBlue);
            MappedResourceView<BgraByte> readView = GD.Map<BgraByte>(readback, MapMode.Read);
            for (uint y = 0; y < h; y++)
            for (uint x = 0; x < w; x++)
            {
                Assert.Equal(clearColor, readView[x, y]);
            }
            GD.Unmap(readback);
        }

        [Fact]
        public void ReadbackSwapchainFramebuffer_ToArrayTexture_Succeedes()
        {
            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetFramebuffer(GD.SwapchainFramebuffer);
            cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            Texture colorTarget = GD.SwapchainFramebuffer.ColorTargets[0].Target;
            (uint w, uint h) = (colorTarget.Width, colorTarget.Height);
            Texture readback = RF.CreateTexture(TextureDescription.Texture2D(
                w, h, 1, 10, colorTarget.Format, TextureUsage.Staging));

            cl.Begin();
            cl.SetFramebuffer(GD.SwapchainFramebuffer);
            cl.CopyTexture(
                colorTarget, 0, 0, 0, 0, 0,
                readback, 0, 0, 0, 0, dstBaseArrayLayer: 5, w, h, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            BgraByte clearColor = new BgraByte(RgbaByte.CornflowerBlue);
            MappedResourceView<BgraByte> readView = GD.Map<BgraByte>(readback, MapMode.Read, 5);

            for (uint y = 0; y < h; y++)
            for (uint x = 0; x < w; x++)
            {
                Assert.Equal(clearColor, readView[x, y]);
            }
            GD.Unmap(readback, 5);
        }

        [Fact]
        public void ReadbackSwapchainFramebuffer_WithOffsets_Succeedes()
        {
            Texture colorTarget = GD.SwapchainFramebuffer.ColorTargets[0].Target;
            (uint w, uint h) = (colorTarget.Width, colorTarget.Height);

            Texture staging = RF.CreateTexture(TextureDescription.Texture2D(
                w, h, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));
            Texture src = RF.CreateTexture(TextureDescription.Texture2D(
                w, h, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            Texture readback = RF.CreateTexture(TextureDescription.Texture2D(
                w, h, 1, 1, colorTarget.Format, TextureUsage.Staging));

            MappedResourceView<RgbaByte> srcData = GD.Map<RgbaByte>(staging, MapMode.Write);
            for (int y = 0; y < staging.Height; y++)
            for (int x = 0; x < staging.Width; x++)
            {
                srcData[x, y] = GD.IsUvOriginTopLeft && !GD.IsClipSpaceYInverted
                    ? new RgbaByte((byte)x, (byte)y, 0, 255)
                    : new RgbaByte((byte)x, (byte)(staging.Height - y - 1), 0, 255);
            }

            GD.Unmap(staging);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(staging, src);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            cl.Begin();
            cl.SetFramebuffer(GD.SwapchainFramebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Clear);
            BlitTexture(GD, cl, GD.SwapchainFramebuffer, src);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            cl.Begin();
            cl.CopyTexture(colorTarget, readback);
            cl.CopyTexture(
                colorTarget,
                50, 50, 0, 0, 0,
                readback,
                10, 10, 0, 0, 0,
                50, 50, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<BgraByte> readView = GD.Map<BgraByte>(readback, MapMode.Read);
            for (int y = 10; y < 60; y++)
            for (int x = 10; x < 60; x++)
            {
                Assert.Equal(new BgraByte(0, (byte)(y + 40), (byte)(x + 40), 255), readView[x, y]);
            }
            GD.Unmap(readback);
        }

        private static void BlitTexture(GraphicsDevice gd, CommandList cl, Framebuffer framebuffer, Texture texture)
        {
            ResourceFactory rf = gd.ResourceFactory;
            ResourceLayout graphicsLayout = rf.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Input", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("InputSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            ResourceSet graphicsSet = rf.CreateResourceSet(new ResourceSetDescription(graphicsLayout, texture, gd.PointSampler));

            Pipeline graphicsPipeline = rf.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleStrip,
                new ShaderSetDescription(
                    Array.Empty<VertexLayoutDescription>(),
                    TestShaders.LoadVertexFragment(rf, "FullScreenBlit")),
                graphicsLayout,
                framebuffer.OutputDescription));

            cl.SetPipeline(graphicsPipeline);
            cl.SetGraphicsResourceSet(0, graphicsSet);
            cl.Draw(4);

            gd.DisposeWhenIdle(graphicsPipeline);
            gd.DisposeWhenIdle(graphicsSet);
            gd.DisposeWhenIdle(graphicsLayout);
        }

        private struct BgraByte
        {
            public byte B;
            public byte G;
            public byte R;
            public byte A;

            public BgraByte(byte b, byte g, byte r, byte a)
                => (B, G, R, A) = (b, g, r, a);

            public BgraByte(RgbaByte rgbaByte)
                => (B, G, R, A) = (rgbaByte.B, rgbaByte.G, rgbaByte.R, rgbaByte.A);
        }
    }

#if TEST_OPENGL
    [Trait("Backend", "OpenGL")]
    public class OpenGLFramebufferTests : FramebufferTests<OpenGLDeviceCreator> { }
    [Trait("Backend", "OpenGL")]
    public class OpenGLSwapchainFramebufferTests : SwapchainFramebufferTests<OpenGLDeviceCreator> { }
#endif
#if TEST_OPENGLES
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESFramebufferTests : FramebufferTests<OpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESSwapchainFramebufferTests : SwapchainFramebufferTests<OpenGLESDeviceCreator> { }
#endif
#if TEST_VULKAN
    [Trait("Backend", "Vulkan")]
    public class VulkanFramebufferTests : FramebufferTests<VulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanSwapchainFramebufferTests : SwapchainFramebufferTests<VulkanDeviceCreatorWithMainSwapchain> { }
#endif
#if TEST_D3D11
    [Trait("Backend", "D3D11")]
    public class D3D11FramebufferTests : FramebufferTests<D3D11DeviceCreator> { }
    [Trait("Backend", "D3D11")]
    public class D3D11SwapchainFramebufferTests : SwapchainFramebufferTests<D3D11DeviceCreatorWithMainSwapchain> { }
#endif
#if TEST_METAL
    [Trait("Backend", "Metal")]
    public class MetalFramebufferTests : FramebufferTests<MetalDeviceCreator> { }
    [Trait("Backend", "Metal")]
    public class MetalSwapchainFramebufferTests : SwapchainFramebufferTests<MetalDeviceCreatorWithMainSwapchain> { }
#endif
}
