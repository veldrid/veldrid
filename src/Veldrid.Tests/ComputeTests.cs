using System.Linq;
using System.Runtime.CompilerServices;
using Veldrid.Tests.Shaders;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class ComputeTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void BasicCompute()
        {
            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Params", ResourceKind.UniformBuffer, ShaderStages.Compute),
                new ResourceLayoutElementDescription("Source", ResourceKind.StructuredBufferReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("Destination", ResourceKind.StructuredBufferReadWrite, ShaderStages.Compute)));

            uint width = 1024;
            uint height = 1024;
            DeviceBuffer paramsBuffer = RF.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<ComputeTestParams>(), BufferUsage.UniformBuffer));
            DeviceBuffer sourceBuffer = RF.CreateBuffer(new BufferDescription(width * height * 4, BufferUsage.StructuredBufferReadWrite, 4));
            DeviceBuffer destinationBuffer = RF.CreateBuffer(new BufferDescription(width * height * 4, BufferUsage.StructuredBufferReadWrite, 4));

            GD.UpdateBuffer(paramsBuffer, 0, new ComputeTestParams { Width = width, Height = height });

            float[] sourceData = new float[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int index = y * (int)width + x;
                    sourceData[index] = index;
                }
            GD.UpdateBuffer(sourceBuffer, 0, sourceData);

            ResourceSet rs = RF.CreateResourceSet(new ResourceSetDescription(layout, paramsBuffer, sourceBuffer, destinationBuffer));

            Pipeline pipeline = RF.CreateComputePipeline(new ComputePipelineDescription(
                TestShaders.Load(RF, "BasicComputeTest", ShaderStages.Compute, "CS"),
                layout,
                16, 16, 1));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetPipeline(pipeline);
            cl.SetComputeResourceSet(0, rs);
            cl.Dispatch(width / 16, width / 16, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            DeviceBuffer sourceReadback = GetReadback(sourceBuffer);
            DeviceBuffer destinationReadback = GetReadback(destinationBuffer);

            MappedResourceView<float> sourceReadView = GD.Map<float>(sourceReadback, MapMode.Read);
            MappedResourceView<float> destinationReadView = GD.Map<float>(destinationReadback, MapMode.Read);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int index = y * (int)width + x;
                    Assert.Equal(2 * sourceData[index], sourceReadView[index]);
                    Assert.Equal(sourceData[index], destinationReadView[index]);
                }

            GD.Unmap(sourceReadback);
            GD.Unmap(destinationReadback);
        }
    }

#if TEST_OPENGL
    public class OpenGLComputeTests : ComputeTests<OpenGLDeviceCreator> { }
#endif
#if TEST_OPENGLES
    public class OpenGLESComputeTests : ComputeTests<OpenGLESDeviceCreator> { }
#endif
#if TEST_VULKAN
    public class VulkanComputeTests : ComputeTests<VulkanDeviceCreatorWithMainSwapchain> { }
#endif
#if TEST_D3D11
    public class D3D11ComputeTests : ComputeTests<D3D11DeviceCreator> { }
#endif
#if TEST_METAL
        public class MetalComputeTests : RenderTests<MetalDeviceCreator> { }
#endif
}
