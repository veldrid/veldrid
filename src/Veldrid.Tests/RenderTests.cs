using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Tests.Shaders;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class RenderTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void Points_WithUIntColor()
        {
            Texture target = RF.CreateTexture(TextureDescription.Texture2D(
                50, 50, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));
            Texture staging = RF.CreateTexture(TextureDescription.Texture2D(
                50, 50, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));

            Framebuffer framebuffer = RF.CreateFramebuffer(new FramebufferDescription(null, target));

            DeviceBuffer infoBuffer = RF.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));
            DeviceBuffer orthoBuffer = RF.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(
                0,
                framebuffer.Width,
                framebuffer.Height,
                0,
                -1,
                1);
            GD.UpdateBuffer(orthoBuffer, 0, ref orthoMatrix);

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                        new VertexElementDescription("Color_UInt", VertexElementSemantic.Color, VertexElementFormat.UInt4))
                },
                new Shader[]
                {
                    TestShaders.Load(RF, "UIntVertexAttribs", ShaderStages.Vertex, "VS"),
                    TestShaders.Load(RF, "UIntVertexAttribs", ShaderStages.Fragment, "FS")
                });

            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("InfoBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Ortho", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceSet set = RF.CreateResourceSet(new ResourceSetDescription(layout, infoBuffer, orthoBuffer));

            GraphicsPipelineDescription gpd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.Default,
                PrimitiveTopology.PointList,
                shaderSet,
                layout,
                framebuffer.OutputDescription);

            Pipeline pipeline = RF.CreateGraphicsPipeline(ref gpd);

            uint colorNormalizationFactor = 2500;

            UIntVertexAttribs.Vertex[] vertices = new UIntVertexAttribs.Vertex[]
            {
                new UIntVertexAttribs.Vertex
                {
                    Position = new Vector2(0.5f, 0.5f),
                    Color_Int = new ShaderGen.UInt4
                    {
                        X = (uint)(0.25f * colorNormalizationFactor),
                        Y = (uint)(0.5f * colorNormalizationFactor),
                        Z = (uint)(0.75f * colorNormalizationFactor),
                    }
                },
                new UIntVertexAttribs.Vertex
                {
                    Position = new Vector2(10.5f, 12.5f),
                    Color_Int = new ShaderGen.UInt4
                    {
                        X = (uint)(0.25f * colorNormalizationFactor),
                        Y = (uint)(0.5f * colorNormalizationFactor),
                        Z = (uint)(0.75f * colorNormalizationFactor),
                    }
                },
                new UIntVertexAttribs.Vertex
                {
                    Position = new Vector2(25.5f, 35.5f),
                    Color_Int = new ShaderGen.UInt4
                    {
                        X = (uint)(0.75f * colorNormalizationFactor),
                        Y = (uint)(0.5f * colorNormalizationFactor),
                        Z = (uint)(0.25f * colorNormalizationFactor),
                    }
                },
                new UIntVertexAttribs.Vertex
                {
                    Position = new Vector2(49.5f, 49.5f),
                    Color_Int = new ShaderGen.UInt4
                    {
                        X = (uint)(0.15f * colorNormalizationFactor),
                        Y = (uint)(0.25f * colorNormalizationFactor),
                        Z = (uint)(0.35f * colorNormalizationFactor),
                    }
                },
            };

            DeviceBuffer vb = RF.CreateBuffer(
                new BufferDescription((uint)(Unsafe.SizeOf<UIntVertexAttribs.Vertex>() * vertices.Length), BufferUsage.VertexBuffer));
            GD.UpdateBuffer(vb, 0, vertices);
            GD.UpdateBuffer(infoBuffer, 0, new UIntVertexAttribs.Info { ColorNormalizationFactor = colorNormalizationFactor });

            CommandList cl = RF.CreateCommandList();

            cl.Begin();
            cl.SetFramebuffer(framebuffer);
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearColorTarget(0, RgbaFloat.Black);
            cl.SetPipeline(pipeline);
            cl.SetVertexBuffer(0, vb);
            cl.SetGraphicsResourceSet(0, set);
            cl.Draw((uint)vertices.Length);
            cl.SetFramebuffer(GD.SwapchainFramebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Red);
            cl.CopyTexture(target, staging);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(staging, MapMode.Read);

            foreach (UIntVertexAttribs.Vertex vertex in vertices)
            {
                uint x = (uint)vertex.Position.X;
                uint y = (uint)vertex.Position.Y;
                if (GD.BackendType == GraphicsBackend.OpenGL)
                {
                    y = framebuffer.Height - y - 1;
                }

                RgbaFloat expectedColor = new RgbaFloat(
                    vertex.Color_Int.X / (float)colorNormalizationFactor,
                    vertex.Color_Int.Y / (float)colorNormalizationFactor,
                    vertex.Color_Int.Z / (float)colorNormalizationFactor,
                    1);
                Assert.Equal(expectedColor, readView[x, y], RgbaFloatFuzzyComparer.Instance);
            }
            GD.Unmap(staging);
        }

        [Fact]
        public void Points_WithUShortNormColor()
        {
            Texture target = RF.CreateTexture(TextureDescription.Texture2D(
                50, 50, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));
            Texture staging = RF.CreateTexture(TextureDescription.Texture2D(
                50, 50, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));

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

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                        new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.UShort4_Norm))
                },
                new Shader[]
                {
                    TestShaders.Load(RF, "U16NormVertexAttribs", ShaderStages.Vertex, "VS"),
                    TestShaders.Load(RF, "U16NormVertexAttribs", ShaderStages.Fragment, "FS")
                });

            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Ortho", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceSet set = RF.CreateResourceSet(new ResourceSetDescription(layout, orthoBuffer));

            GraphicsPipelineDescription gpd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.Default,
                PrimitiveTopology.PointList,
                shaderSet,
                layout,
                framebuffer.OutputDescription);

            Pipeline pipeline = RF.CreateGraphicsPipeline(ref gpd);

            VertexCPU[] vertices = new VertexCPU[]
            {
                new VertexCPU
                {
                    Position = new Vector2(0.5f, 0.5f),
                    R = UShortNorm(0.25f),
                    G = UShortNorm(0.5f),
                    B = UShortNorm(0.75f),
                },
                new VertexCPU
                {
                    Position = new Vector2(10.5f, 12.5f),
                    R = UShortNorm(0.25f),
                    G = UShortNorm(0.5f),
                    B = UShortNorm(0.75f),
                },
                new VertexCPU
                {
                    Position = new Vector2(25.5f, 35.5f),
                    R = UShortNorm(0.75f),
                    G = UShortNorm(0.5f),
                    B = UShortNorm(0.25f),
                },
                new VertexCPU
                {
                    Position = new Vector2(49.5f, 49.5f),
                    R = UShortNorm(0.15f),
                    G = UShortNorm(0.25f),
                    B = UShortNorm(0.35f),
                },
            };

            DeviceBuffer vb = RF.CreateBuffer(
                new BufferDescription((uint)(Unsafe.SizeOf<VertexCPU>() * vertices.Length), BufferUsage.VertexBuffer));
            GD.UpdateBuffer(vb, 0, vertices);

            CommandList cl = RF.CreateCommandList();

            cl.Begin();
            cl.SetFramebuffer(framebuffer);
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearColorTarget(0, RgbaFloat.Black);
            cl.SetPipeline(pipeline);
            cl.SetVertexBuffer(0, vb);
            cl.SetGraphicsResourceSet(0, set);
            cl.Draw((uint)vertices.Length);
            cl.SetFramebuffer(GD.SwapchainFramebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Red);
            cl.CopyTexture(target, staging);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(staging, MapMode.Read);

            foreach (VertexCPU vertex in vertices)
            {
                uint x = (uint)vertex.Position.X;
                uint y = (uint)vertex.Position.Y;
                if (GD.BackendType == GraphicsBackend.OpenGL)
                {
                    y = framebuffer.Height - y - 1;
                }

                RgbaFloat expectedColor = new RgbaFloat(
                    vertex.R / (float)ushort.MaxValue,
                    vertex.G / (float)ushort.MaxValue,
                    vertex.B / (float)ushort.MaxValue,
                    1);
                Assert.Equal(expectedColor, readView[x, y], RgbaFloatFuzzyComparer.Instance);
            }
            GD.Unmap(staging);
        }

        public struct VertexCPU
        {
            public Vector2 Position;
            public ushort R;
            public ushort G;
            public ushort B;
            public ushort A;
        }

        private ushort UShortNorm(float normalizedValue)
        {
            Debug.Assert(normalizedValue >= 0 && normalizedValue <= 1);
            return (ushort)(normalizedValue * ushort.MaxValue);
        }
    }

    public class OpenGLRenderTests : RenderTests<OpenGLDeviceCreator> { }
#if TEST_VULKAN
    public class VulkanRenderTests : RenderTests<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    public class D3D11RenderTests : RenderTests<D3D11DeviceCreator> { }
#endif
#if TEST_METAL
        public class MetalRenderTests : RenderTests<MetalDeviceCreator> { }
#endif
}
