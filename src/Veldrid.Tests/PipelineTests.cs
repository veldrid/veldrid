using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class PipelineTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [SkippableFact]
        public void CreatePipelines_DifferentInstanceStepRate_Succeeds()
        {
            Texture colorTex = RF.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget));
            Framebuffer framebuffer = RF.CreateFramebuffer(new FramebufferDescription(null, colorTex));

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    new VertexLayoutDescription(
                        24,
                        0,
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("Color_UInt", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4))
                },
                TestShaders.LoadVertexFragment(RF, "UIntVertexAttribs"));

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
                framebuffer.OutputDescription);

            Pipeline pipeline1 = RF.CreateGraphicsPipeline(ref gpd);
            Pipeline pipeline2 = RF.CreateGraphicsPipeline(ref gpd);

            gpd.ShaderSet.VertexLayouts[0].InstanceStepRate = 4;
            Pipeline pipeline3 = RF.CreateGraphicsPipeline(ref gpd);

            gpd.ShaderSet.VertexLayouts[0].InstanceStepRate = 5;
            Pipeline pipeline4 = RF.CreateGraphicsPipeline(ref gpd);
        }
    }

    [Trait("Backend", "OpenGL")]
    public class OpenGLPipelineTests : PipelineTests<OpenGLDeviceCreator> { }

    [Trait("Backend", "OpenGLES")]
    public class OpenGLESPipelineTests : PipelineTests<OpenGLESDeviceCreator> { }

    [Trait("Backend", "Vulkan")]
    public class VulkanPipelineTests : PipelineTests<VulkanDeviceCreator> { }

    [Trait("Backend", "D3D11")]
    public class D3D11PipelineTests : PipelineTests<D3D11DeviceCreator> { }

    [Trait("Backend", "Metal")]
    public class MetalPipelineTests : PipelineTests<MetalDeviceCreator> { }
}
