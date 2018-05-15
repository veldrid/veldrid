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
            cl.CopyTexture(target, staging);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(staging, MapMode.Read);

            foreach (UIntVertexAttribs.Vertex vertex in vertices)
            {
                uint x = (uint)vertex.Position.X;
                uint y = (uint)vertex.Position.Y;
                if (HasInvertedY(GD.BackendType))
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

            VertexCPU_UShortNorm[] vertices = new VertexCPU_UShortNorm[]
            {
                new VertexCPU_UShortNorm
                {
                    Position = new Vector2(0.5f, 0.5f),
                    R = UShortNorm(0.25f),
                    G = UShortNorm(0.5f),
                    B = UShortNorm(0.75f),
                },
                new VertexCPU_UShortNorm
                {
                    Position = new Vector2(10.5f, 12.5f),
                    R = UShortNorm(0.25f),
                    G = UShortNorm(0.5f),
                    B = UShortNorm(0.75f),
                },
                new VertexCPU_UShortNorm
                {
                    Position = new Vector2(25.5f, 35.5f),
                    R = UShortNorm(0.75f),
                    G = UShortNorm(0.5f),
                    B = UShortNorm(0.25f),
                },
                new VertexCPU_UShortNorm
                {
                    Position = new Vector2(49.5f, 49.5f),
                    R = UShortNorm(0.15f),
                    G = UShortNorm(0.25f),
                    B = UShortNorm(0.35f),
                },
            };

            DeviceBuffer vb = RF.CreateBuffer(
                new BufferDescription((uint)(Unsafe.SizeOf<VertexCPU_UShortNorm>() * vertices.Length), BufferUsage.VertexBuffer));
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
            cl.CopyTexture(target, staging);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(staging, MapMode.Read);

            foreach (VertexCPU_UShortNorm vertex in vertices)
            {
                uint x = (uint)vertex.Position.X;
                uint y = (uint)vertex.Position.Y;
                if (HasInvertedY(GD.BackendType))
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

        public struct VertexCPU_UShortNorm
        {
            public Vector2 Position;
            public ushort R;
            public ushort G;
            public ushort B;
            public ushort A;
        }

        public struct VertexCPU_UShort
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

        [Fact]
        public void Points_WithUShortColor()
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
                        new VertexElementDescription("Color_UInt", VertexElementSemantic.Color, VertexElementFormat.UShort4))
                },
                new Shader[]
                {
                    TestShaders.Load(RF, "U16VertexAttribs", ShaderStages.Vertex, "VS"),
                    TestShaders.Load(RF, "U16VertexAttribs", ShaderStages.Fragment, "FS")
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

            VertexCPU_UShort[] vertices = new VertexCPU_UShort[]
            {
                new VertexCPU_UShort
                {
                    Position = new Vector2(0.5f, 0.5f),
                    R = (ushort)(0.25f * colorNormalizationFactor),
                    G = (ushort)(0.5f * colorNormalizationFactor),
                    B = (ushort)(0.75f * colorNormalizationFactor),
                },
                new VertexCPU_UShort
                {
                    Position = new Vector2(10.5f, 12.5f),
                    R = (ushort)(0.25f * colorNormalizationFactor),
                    G = (ushort)(0.5f * colorNormalizationFactor),
                    B = (ushort)(0.75f * colorNormalizationFactor),
                },
                new VertexCPU_UShort
                {
                    Position = new Vector2(25.5f, 35.5f),
                    R = (ushort)(0.75f * colorNormalizationFactor),
                    G = (ushort)(0.5f * colorNormalizationFactor),
                    B = (ushort)(0.25f * colorNormalizationFactor),
                },
                new VertexCPU_UShort
                {
                    Position = new Vector2(49.5f, 49.5f),
                    R = (ushort)(0.15f * colorNormalizationFactor),
                    G = (ushort)(0.2f * colorNormalizationFactor),
                    B = (ushort)(0.35f * colorNormalizationFactor),
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
            cl.CopyTexture(target, staging);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(staging, MapMode.Read);

            foreach (VertexCPU_UShort vertex in vertices)
            {
                uint x = (uint)vertex.Position.X;
                uint y = (uint)vertex.Position.Y;
                if (HasInvertedY(GD.BackendType))
                {
                    y = framebuffer.Height - y - 1;
                }

                RgbaFloat expectedColor = new RgbaFloat(
                    vertex.R / (float)colorNormalizationFactor,
                    vertex.G / (float)colorNormalizationFactor,
                    vertex.B / (float)colorNormalizationFactor,
                    1);
                Assert.Equal(expectedColor, readView[x, y], RgbaFloatFuzzyComparer.Instance);
            }
            GD.Unmap(staging);
        }

        [Fact]
        public unsafe void Points_WithTexture_UpdateUnrelated()
        {
            // This is a regression test for the case where a user modifies an unrelated texture
            // at a time after a ResourceSet containing a texture has been bound. The OpenGL
            // backend was caching texture state improperly, resulting in wrong textures being sampled.

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

            Texture sampledTexture = RF.CreateTexture(
                TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled));
            TextureView view = RF.CreateTextureView(sampledTexture);
            RgbaFloat white = RgbaFloat.White;
            GD.UpdateTexture(sampledTexture, (IntPtr)(&white), (uint)Unsafe.SizeOf<RgbaFloat>(), 0, 0, 0, 1, 1, 1, 0, 0);

            Texture shouldntBeSampledTexture = RF.CreateTexture(
                TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled));

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2))
                },
                new Shader[]
                {
                    TestShaders.Load(RF, "TexturedPoints", ShaderStages.Vertex, "VS"),
                    TestShaders.Load(RF, "TexturedPoints", ShaderStages.Fragment, "FS")
                });

            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Ortho", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Smp", ResourceKind.Sampler, ShaderStages.Fragment)));

            ResourceSet set = RF.CreateResourceSet(new ResourceSetDescription(layout, orthoBuffer, view, GD.PointSampler));

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

            for (int i = 0; i < 2; i++)
            {
                cl.Begin();
                cl.SetFramebuffer(framebuffer);
                cl.ClearColorTarget(0, RgbaFloat.Black);
                cl.SetPipeline(pipeline);
                cl.SetVertexBuffer(0, vb);
                cl.SetGraphicsResourceSet(0, set);

                // Modify an unrelated texture.
                // This must have no observable effect on the next draw call.
                RgbaFloat pink = RgbaFloat.Pink;
                GD.UpdateTexture(shouldntBeSampledTexture,
                    (IntPtr)(&pink), (uint)Unsafe.SizeOf<RgbaFloat>(),
                    0, 0, 0,
                    1, 1, 1,
                    0, 0);

                cl.Draw((uint)vertices.Length);
                cl.End();
                GD.SubmitCommands(cl);
                GD.WaitForIdle();
            }

            cl.Begin();
            cl.CopyTexture(target, staging);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(staging, MapMode.Read);

            foreach (Vector2 vertex in vertices)
            {
                uint x = (uint)vertex.X;
                uint y = (uint)vertex.Y;
                if (HasInvertedY(GD.BackendType))
                {
                    y = framebuffer.Height - y - 1;
                }

                Assert.Equal(white, readView[x, y], RgbaFloatFuzzyComparer.Instance);
            }
            GD.Unmap(staging);
        }

        [Fact]
        public void ComputeGeneratedVertices()
        {
            if (!GD.Features.ComputeShader)
            {
                return;
            }

            uint width = 512;
            uint height = 512;
            Texture output = RF.CreateTexture(
                TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));
            Framebuffer framebuffer = RF.CreateFramebuffer(new FramebufferDescription(null, output));

            uint vertexSize = (uint)Unsafe.SizeOf<ColoredVertex>();
            DeviceBuffer buffer = RF.CreateBuffer(new BufferDescription(
                vertexSize * 4,
                BufferUsage.StructuredBufferReadWrite,
                vertexSize));

            ResourceLayout computeLayout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("OutputVertices", ResourceKind.StructuredBufferReadWrite, ShaderStages.Compute)));
            ResourceSet computeSet = RF.CreateResourceSet(new ResourceSetDescription(computeLayout, buffer));

            Pipeline computePipeline = RF.CreateComputePipeline(new ComputePipelineDescription(
                TestShaders.Load(RF, "ComputeColoredQuadGenerator", ShaderStages.Compute, "CS"),
                computeLayout,
                1, 1, 1));

            ResourceLayout graphicsLayout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("InputVertices", ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex)));
            ResourceSet graphicsSet = RF.CreateResourceSet(new ResourceSetDescription(graphicsLayout, buffer));

            Pipeline graphicsPipeline = RF.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleStrip,
                new ShaderSetDescription(
                    Array.Empty<VertexLayoutDescription>(),
                    new[]
                    {
                        TestShaders.Load(RF, "ColoredQuadRenderer", ShaderStages.Vertex, "VS"),
                        TestShaders.Load(RF, "ColoredQuadRenderer", ShaderStages.Fragment, "FS"),
                    }),
                graphicsLayout,
                framebuffer.OutputDescription));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetPipeline(computePipeline);
            cl.SetComputeResourceSet(0, computeSet);
            cl.Dispatch(1, 1, 1);
            cl.SetFramebuffer(framebuffer);
            cl.ClearColorTarget(0, new RgbaFloat());
            cl.SetPipeline(graphicsPipeline);
            cl.SetGraphicsResourceSet(0, graphicsSet);
            cl.Draw(4);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            Texture readback = GetReadback(output);
            MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(readback, MapMode.Read);
            for (uint y = 0; y < height; y++)
                for (uint x = 0; x < width; x++)
                {
                    Assert.Equal(RgbaFloat.Red, readView[x, y]);
                }
            GD.Unmap(readback);
        }

        private bool HasInvertedY(GraphicsBackend backendType)
        {
            return backendType == GraphicsBackend.OpenGL || backendType == GraphicsBackend.OpenGLES;
        }
    }

#if TEST_OPENGL
    public class OpenGLRenderTests : RenderTests<OpenGLDeviceCreator> { }
#endif
#if TEST_OPENGLES
    public class OpenGLESRenderTests : RenderTests<OpenGLESDeviceCreator> { }
#endif
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
