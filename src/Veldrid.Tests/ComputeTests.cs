using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace Veldrid.Tests
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BasicComputeTestParams
    {
        public uint Width;
        public uint Height;
        private uint _padding1;
        private uint _padding2;
    }

    public abstract class ComputeTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void BasicCompute()
        {
            if (!GD.Features.ComputeShader)
            {
                return;
            }

            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Params", ResourceKind.UniformBuffer, ShaderStages.Compute),
                new ResourceLayoutElementDescription("Source", ResourceKind.StructuredBufferReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("Destination", ResourceKind.StructuredBufferReadWrite, ShaderStages.Compute)));

            uint width = 1024;
            uint height = 1024;
            DeviceBuffer paramsBuffer = RF.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<BasicComputeTestParams>(), BufferUsage.UniformBuffer));
            DeviceBuffer sourceBuffer = RF.CreateBuffer(new BufferDescription(width * height * 4, BufferUsage.StructuredBufferReadWrite, 4));
            DeviceBuffer destinationBuffer = RF.CreateBuffer(new BufferDescription(width * height * 4, BufferUsage.StructuredBufferReadWrite, 4));

            GD.UpdateBuffer(paramsBuffer, 0, new BasicComputeTestParams { Width = width, Height = height });

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
                TestShaders.LoadCompute(RF, "BasicComputeTest"),
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

        [Theory]
        [MemberData(nameof(FillBuffer_WithOffsetsData))]
        public void FillBuffer_WithOffsets(uint srcSetMultiple, uint srcBindingMultiple, uint dstSetMultiple, uint dstBindingMultiple, bool combinedLayout)
        {
            if (!GD.Features.ComputeShader) { return; }
            Debug.Assert((GD.StructuredBufferMinOffsetAlignment % sizeof(uint)) == 0);

            uint valueCount = 512;
            uint dataSize = valueCount * sizeof(uint);
            uint totalSrcAlignment = GD.StructuredBufferMinOffsetAlignment * (srcSetMultiple + srcBindingMultiple);
            uint totalDstAlignment = GD.StructuredBufferMinOffsetAlignment * (dstSetMultiple + dstBindingMultiple);

            DeviceBuffer copySrc = RF.CreateBuffer(
                new BufferDescription(totalSrcAlignment + dataSize, BufferUsage.StructuredBufferReadOnly, sizeof(uint)));
            DeviceBuffer copyDst = RF.CreateBuffer(
                new BufferDescription(totalDstAlignment + dataSize, BufferUsage.StructuredBufferReadWrite, sizeof(uint)));

            ResourceLayout[] layouts;
            ResourceSet[] sets;

            DeviceBufferRange srcRange = new DeviceBufferRange(copySrc, srcSetMultiple * GD.StructuredBufferMinOffsetAlignment, dataSize);
            DeviceBufferRange dstRange = new DeviceBufferRange(copyDst, dstSetMultiple * GD.StructuredBufferMinOffsetAlignment, dataSize);

            if (combinedLayout)
            {
                layouts = new[]
                {
                    RF.CreateResourceLayout(new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription(
                            "CopySrc",
                            ResourceKind.StructuredBufferReadOnly,
                            ShaderStages.Compute,
                            ResourceLayoutElementOptions.DynamicBinding),
                        new ResourceLayoutElementDescription(
                            "CopyDst",
                            ResourceKind.StructuredBufferReadWrite,
                            ShaderStages.Compute,
                            ResourceLayoutElementOptions.DynamicBinding)))
                };
                sets = new[]
                {
                    RF.CreateResourceSet(new ResourceSetDescription(layouts[0], srcRange, dstRange))
                };
            }
            else
            {
                layouts = new[]
                {
                    RF.CreateResourceLayout(new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription(
                            "CopySrc",
                            ResourceKind.StructuredBufferReadOnly,
                            ShaderStages.Compute,
                            ResourceLayoutElementOptions.DynamicBinding))),
                    RF.CreateResourceLayout(new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription(
                            "CopyDst",
                            ResourceKind.StructuredBufferReadWrite,
                            ShaderStages.Compute,
                            ResourceLayoutElementOptions.DynamicBinding)))
                };
                sets = new[]
                {
                    RF.CreateResourceSet(new ResourceSetDescription(layouts[0], srcRange)),
                    RF.CreateResourceSet(new ResourceSetDescription(layouts[1], dstRange)),
                };
            }

            Pipeline pipeline = RF.CreateComputePipeline(new ComputePipelineDescription(
                TestShaders.LoadCompute(RF, combinedLayout ? "FillBuffer" : "FillBuffer_SeparateLayout"),
                layouts,
                1, 1, 1));

            uint[] srcData = Enumerable.Range(0, (int)copySrc.SizeInBytes / sizeof(uint)).Select(i => (uint)i).ToArray();
            GD.UpdateBuffer(copySrc, 0, srcData);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetPipeline(pipeline);
            if (combinedLayout)
            {
                uint[] offsets = new[]
                {
                    srcBindingMultiple * GD.StructuredBufferMinOffsetAlignment,
                    dstBindingMultiple * GD.StructuredBufferMinOffsetAlignment
                };
                cl.SetComputeResourceSet(0, sets[0], offsets);
            }
            else
            {
                uint offset = srcBindingMultiple * GD.StructuredBufferMinOffsetAlignment;
                cl.SetComputeResourceSet(0, sets[0], 1, ref offset);
                offset = dstBindingMultiple * GD.StructuredBufferMinOffsetAlignment;
                cl.SetComputeResourceSet(1, sets[1], 1, ref offset);
            }
            cl.Dispatch(512, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            DeviceBuffer readback = GetReadback(copyDst);

            MappedResourceView<uint> readView = GD.Map<uint>(readback, MapMode.Read);
            for (uint i = 0; i < valueCount; i++)
            {
                uint srcIndex = totalSrcAlignment / sizeof(uint) + i;
                uint expected = srcData[(int)srcIndex];

                uint dstIndex = totalDstAlignment / sizeof(uint) + i;
                uint actual = readView[dstIndex];

                Assert.Equal(expected, actual);
            }
            GD.Unmap(readback);
        }

        public static IEnumerable<object[]> FillBuffer_WithOffsetsData()
        {
            foreach (uint srcSetMultiple in new[] { 0, 2, 10 })
                foreach (uint srcBindingMultiple in new[] { 0, 2, 10 })
                    foreach (uint dstSetMultiple in new[] { 0, 2, 10 })
                        foreach (uint dstBindingMultiple in new[] { 0, 2, 10 })
                            foreach (bool combinedLayout in new[] { false, true })
                            {
                                yield return new object[] { srcSetMultiple, srcBindingMultiple, dstSetMultiple, dstBindingMultiple, combinedLayout };
                            }
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
    public class D3D11ComputeTests : ComputeTests<D3D11DeviceCreatorWithMainSwapchain> { }
#endif
#if TEST_METAL
        public class MetalComputeTests : RenderTests<MetalDeviceCreator> { }
#endif
}
