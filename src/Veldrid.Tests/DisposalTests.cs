using Xunit;

namespace Veldrid.Tests
{
    public abstract class DisposalTestBase<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void Dispose_Buffer()
        {
            DeviceBuffer b = RF.CreateBuffer(new BufferDescription(256, BufferUsage.VertexBuffer));
            b.Dispose();
            Assert.True(b.IsDisposed);
        }

        [Fact]
        public void Dispose_Texture()
        {
            Texture t = RF.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled));
            TextureView tv = RF.CreateTextureView(t);
            GD.WaitForIdle(); // Required currently by Vulkan backend.
            tv.Dispose();
            Assert.True(tv.IsDisposed);
            Assert.False(t.IsDisposed);
            t.Dispose();
            Assert.True(t.IsDisposed);
        }

        [Fact]
        public void Dispose_Framebuffer()
        {
            Texture t = RF.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));
            Framebuffer fb = RF.CreateFramebuffer(new FramebufferDescription(null, t));
            GD.WaitForIdle(); // Required currently by Vulkan backend.
            fb.Dispose();
            Assert.True(fb.IsDisposed);
            Assert.False(t.IsDisposed);
            t.Dispose();
            Assert.True(t.IsDisposed);
        }

        [Fact]
        public void Dispose_CommandList()
        {
            CommandList cl = RF.CreateCommandList();
            cl.Dispose();
            Assert.True(cl.IsDisposed);
        }

        [Fact]
        public void Dispose_Sampler()
        {
            Sampler s = RF.CreateSampler(SamplerDescription.Point);
            s.Dispose();
            Assert.True(s.IsDisposed);
        }

        [Fact]
        public void Dispose_Pipeline()
        {
            Shader[] shaders = TestShaders.LoadVertexFragment(RF, "UIntVertexAttribs");
            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("Color_UInt", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4))
                },
                shaders);

            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("InfoBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Ortho", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            GraphicsPipelineDescription gpd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.Default,
                PrimitiveTopology.PointList,
                shaderSet,
                layout,
                new OutputDescription(null, new OutputAttachmentDescription(PixelFormat.R32_G32_B32_A32_Float)));
            Pipeline pipeline = RF.CreateGraphicsPipeline(ref gpd);
            pipeline.Dispose();
            Assert.True(pipeline.IsDisposed);
            Assert.False(shaders[0].IsDisposed);
            Assert.False(shaders[1].IsDisposed);
            Assert.False(layout.IsDisposed);
            layout.Dispose();
            Assert.True(layout.IsDisposed);
            Assert.False(shaders[0].IsDisposed);
            Assert.False(shaders[1].IsDisposed);
            shaders[0].Dispose();
            Assert.True(shaders[0].IsDisposed);
            shaders[1].Dispose();
            Assert.True(shaders[1].IsDisposed);
        }

        [Fact]
        public void Dispose_ResourceSet()
        {
            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("InfoBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Ortho", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            DeviceBuffer ub0 = RF.CreateBuffer(new BufferDescription(256, BufferUsage.UniformBuffer));
            DeviceBuffer ub1 = RF.CreateBuffer(new BufferDescription(256, BufferUsage.UniformBuffer));

            ResourceSet rs = RF.CreateResourceSet(new ResourceSetDescription(layout, ub0, ub1));
            rs.Dispose();
            Assert.True(rs.IsDisposed);
            Assert.False(ub0.IsDisposed);
            Assert.False(ub1.IsDisposed);
            Assert.False(layout.IsDisposed);
            layout.Dispose();
            Assert.True(layout.IsDisposed);
            Assert.False(ub0.IsDisposed);
            Assert.False(ub1.IsDisposed);
            ub0.Dispose();
            Assert.True(ub0.IsDisposed);
            ub1.Dispose();
            Assert.True(ub1.IsDisposed);
        }
    }

#if TEST_VULKAN
    [Trait("Backend", "Vulkan")]
    public class VulkanDisposalTests : DisposalTestBase<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    [Trait("Backend", "D3D11")]
    public class D3D11DisposalTests : DisposalTestBase<D3D11DeviceCreator> { }
#endif
#if TEST_METAL
    [Trait("Backend", "Metal")]
    public class MetalDisposalTests : DisposalTestBase<MetalDeviceCreator> { }
#endif
#if TEST_OPENGL
    [Trait("Backend", "OpenGL")]
    public class OpenGLDisposalTests : DisposalTestBase<OpenGLDeviceCreator> { }
#endif
#if TEST_OPENGLES
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESDisposalTests : DisposalTestBase<OpenGLESDeviceCreator> { }
#endif
}
