using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class ReflectionTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public unsafe void UnusedResourceSlots_ReflectionInfo()
        {
            Texture target = RF.CreateTexture(TextureDescription.Texture2D(
                50, 50, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));

            Framebuffer framebuffer = RF.CreateFramebuffer(new FramebufferDescription(null, target));

            DeviceBuffer orthoBuffer = RF.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(
                0,
                framebuffer.Width,
                framebuffer.Height,
                0,
                -1,
                1);
            GD.UpdateBuffer(orthoBuffer, 0, ref orthoMatrix);

            Texture sampledTexture = RF.CreateTexture(
                TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled));

            GD.UpdateTexture(sampledTexture, new[] { RgbaFloat.White }, 0, 0, 0, 1, 1, 1, 0, 0);

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
                },
                TestShaders.LoadVertexFragment(RF, "UnusedResourceSlots", loadSpirv: true));

            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("UnusedBuffer_0", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("UnusedBuffer_1", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("OrthoBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Unused_Tex_0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Unused_Tex_1", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Smp", ResourceKind.Sampler, ShaderStages.Fragment)));

            DeviceBuffer dummyBuf = RF.CreateBuffer(
                new BufferDescription(128, BufferUsage.UniformBuffer));
            Texture dummyTex = RF.CreateTexture(
                new TextureDescription(1, 1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));

            ResourceSet set = RF.CreateResourceSet(
                new ResourceSetDescription(layout, dummyBuf, dummyBuf, orthoBuffer, dummyTex, dummyTex, sampledTexture, GD.PointSampler));

            GraphicsPipelineDescription gpd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.Default,
                PrimitiveTopology.PointList,
                shaderSet,
                layout,
                framebuffer.OutputDescription);

            gpd.ReflectedResourceLayouts = new ResourceLayoutDescription[]
            {
                new ResourceLayoutDescription(
                    ResourceLayoutElementDescription.Unused,
                    ResourceLayoutElementDescription.Unused,
                    new ResourceLayoutElementDescription("_21_23", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    ResourceLayoutElementDescription.Unused,
                    ResourceLayoutElementDescription.Unused,
                    new ResourceLayoutElementDescription("_25", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("_25_Smp", ResourceKind.Sampler, ShaderStages.Fragment))
            };

            gpd.ReflectedVertexElements = new[]
            {
                new VertexElementDescription("_29", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            };

            Pipeline pipeline = RF.CreateGraphicsPipeline(ref gpd);

            Vector2[] vertices = new Vector2[]
            {
                new Vector2(0.5f, 0.5f),
                new Vector2(15.5f, 15.5f),
                new Vector2(25.5f, 26.5f),
                new Vector2(3.5f, 25.5f),
            };

            DeviceBuffer vb = RF.CreateBuffer(
                new BufferDescription((uint)(Unsafe.SizeOf<Vector2>() * vertices.Length), BufferUsage.VertexBuffer));
            GD.UpdateBuffer(vb, 0, vertices);

            CommandList cl = RF.CreateCommandList();

            cl.Begin();
            cl.SetFramebuffer(framebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Black);
            cl.SetPipeline(pipeline);
            cl.SetVertexBuffer(0, vb);
            cl.SetGraphicsResourceSet(0, set);

            cl.Draw((uint)vertices.Length);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            Texture readback = GetReadback(target);

            MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(readback, MapMode.Read);

            foreach (Vector2 vertex in vertices)
            {
                uint x = (uint)vertex.X;
                uint y = (uint)vertex.Y;
                if (!GD.IsUvOriginTopLeft || GD.IsClipSpaceYInverted)
                {
                    y = framebuffer.Height - y - 1;
                }

                Assert.Equal(RgbaFloat.White, readView[x, y], RgbaFloatFuzzyComparer.Instance);
            }
            GD.Unmap(readback);
        }

        [Fact]
        public unsafe void UnusedResourceSlots_NoReflectionInfo()
        {
            Texture target = RF.CreateTexture(TextureDescription.Texture2D(
                50, 50, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));

            Framebuffer framebuffer = RF.CreateFramebuffer(new FramebufferDescription(null, target));

            DeviceBuffer orthoBuffer = RF.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(
                0,
                framebuffer.Width,
                framebuffer.Height,
                0,
                -1,
                1);
            GD.UpdateBuffer(orthoBuffer, 0, ref orthoMatrix);

            Texture sampledTexture = RF.CreateTexture(
                TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled));

            GD.UpdateTexture(sampledTexture, new[] { RgbaFloat.White }, 0, 0, 0, 1, 1, 1, 0, 0);

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("_29", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
                },
                TestShaders.LoadVertexFragment(RF, "UnusedResourceSlots", loadSpirv: true));

            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                ResourceLayoutElementDescription.Unused,
                ResourceLayoutElementDescription.Unused,
                new ResourceLayoutElementDescription("_21_23", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                ResourceLayoutElementDescription.Unused,
                ResourceLayoutElementDescription.Unused,
                new ResourceLayoutElementDescription("_25", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("_25_Smp", ResourceKind.Sampler, ShaderStages.Fragment)));

            ResourceSet set = RF.CreateResourceSet(
                new ResourceSetDescription(layout, null, null, orthoBuffer, null, null, sampledTexture, GD.PointSampler));

            GraphicsPipelineDescription gpd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.Default,
                PrimitiveTopology.PointList,
                shaderSet,
                layout,
                framebuffer.OutputDescription);

            Pipeline pipeline = RF.CreateGraphicsPipeline(ref gpd);

            Vector2[] vertices = new Vector2[]
            {
                new Vector2(0.5f, 0.5f),
                new Vector2(15.5f, 15.5f),
                new Vector2(25.5f, 26.5f),
                new Vector2(3.5f, 25.5f),
            };

            DeviceBuffer vb = RF.CreateBuffer(
                new BufferDescription((uint)(Unsafe.SizeOf<Vector2>() * vertices.Length), BufferUsage.VertexBuffer));
            GD.UpdateBuffer(vb, 0, vertices);

            CommandList cl = RF.CreateCommandList();

            cl.Begin();
            cl.SetFramebuffer(framebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Black);
            cl.SetPipeline(pipeline);
            cl.SetVertexBuffer(0, vb);
            cl.SetGraphicsResourceSet(0, set);

            cl.Draw((uint)vertices.Length);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            Texture readback = GetReadback(target);

            MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(readback, MapMode.Read);

            foreach (Vector2 vertex in vertices)
            {
                uint x = (uint)vertex.X;
                uint y = (uint)vertex.Y;
                if (!GD.IsUvOriginTopLeft || GD.IsClipSpaceYInverted)
                {
                    y = framebuffer.Height - y - 1;
                }

                Assert.Equal(RgbaFloat.White, readView[x, y], RgbaFloatFuzzyComparer.Instance);
            }
            GD.Unmap(readback);
        }
    }

#if TEST_OPENGL
    public class OpenGLReflectionTests : ReflectionTests<OpenGLDeviceCreator> { }
#endif
#if TEST_OPENGLES
    public class OpenGLESReflectionTests : ReflectionTests<OpenGLESDeviceCreator> { }
#endif
#if TEST_VULKAN
    public class VulkanReflectionTests : ReflectionTests<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    public class D3D11ReflectionTests : ReflectionTests<D3D11DeviceCreator> { }
#endif
#if TEST_METAL
    public class MetalReflectionTests : ReflectionTests<MetalDeviceCreator> { }
#endif
}
