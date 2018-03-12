using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class BufferTestBase<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void CreateBuffer_Succeeds()
        {
            uint expectedSize = 64;
            BufferUsage expectedUsage = BufferUsage.Dynamic | BufferUsage.UniformBuffer;

            DeviceBuffer buffer = RF.CreateBuffer(new BufferDescription(expectedSize, expectedUsage));

            Assert.Equal(expectedUsage, buffer.Usage);
            Assert.Equal(expectedSize, buffer.SizeInBytes);
        }

        [Fact]
        public void UpdateBuffer_NonDynamic_Succeeds()
        {
            DeviceBuffer buffer = CreateBuffer(64, BufferUsage.VertexBuffer);
            GD.UpdateBuffer(buffer, 0, Matrix4x4.Identity);
            GD.WaitForIdle();
        }

        [Fact]
        public void UpdateBuffer_ThenMapRead_Succeeds()
        {
            DeviceBuffer buffer = CreateBuffer(1024, BufferUsage.Staging);
            int[] data = Enumerable.Range(0, 256).Select(i => 2 * i).ToArray();
            GD.UpdateBuffer(buffer, 0, data);

            MappedResourceView<int> view = GD.Map<int>(buffer, MapMode.Read);
            for (int i = 0; i < view.Count; i++)
            {
                Assert.Equal(i * 2, view[i]);
            }
        }

        [Fact]
        public unsafe void Staging_Map_WriteThenRead()
        {
            DeviceBuffer buffer = CreateBuffer(256, BufferUsage.Staging);
            MappedResource map = GD.Map(buffer, MapMode.Write);
            byte* dataPtr = (byte*)map.Data.ToPointer();
            for (int i = 0; i < map.SizeInBytes; i++)
            {
                dataPtr[i] = (byte)i;
            }
            GD.Unmap(buffer);

            map = GD.Map(buffer, MapMode.Read);
            dataPtr = (byte*)map.Data.ToPointer();
            for (int i = 0; i < map.SizeInBytes; i++)
            {
                Assert.Equal((byte)i, dataPtr[i]);
            }
        }

        [Fact]
        public void Staging_MapGeneric_WriteThenRead()
        {
            DeviceBuffer buffer = CreateBuffer(1024, BufferUsage.Staging);
            MappedResourceView<int> view = GD.Map<int>(buffer, MapMode.Write);
            Assert.Equal(256, view.Count);
            for (int i = 0; i < view.Count; i++)
            {
                view[i] = i * 10;
            }
            GD.Unmap(buffer);

            view = GD.Map<int>(buffer, MapMode.Read);
            Assert.Equal(256, view.Count);
            for (int i = 0; i < view.Count; i++)
            {
                view[i] = 1 * 10;
            }
            GD.Unmap(buffer);
        }

        [Fact]
        public void MapGeneric_OutOfBounds_ThrowsIndexOutOfRange()
        {
            DeviceBuffer buffer = CreateBuffer(1024, BufferUsage.Staging);
            MappedResourceView<byte> view = GD.Map<byte>(buffer, MapMode.ReadWrite);
            Assert.Throws<IndexOutOfRangeException>(() => view[1024]);
            Assert.Throws<IndexOutOfRangeException>(() => view[-1]);
        }

        [Fact]
        public void Map_WrongFlags_Throws()
        {
            DeviceBuffer buffer = CreateBuffer(1024, BufferUsage.VertexBuffer);
            Assert.Throws<VeldridException>(() => GD.Map(buffer, MapMode.Read));
            Assert.Throws<VeldridException>(() => GD.Map(buffer, MapMode.Write));
            Assert.Throws<VeldridException>(() => GD.Map(buffer, MapMode.ReadWrite));
        }

        [Fact]
        public void CopyBuffer_Succeeds()
        {
            DeviceBuffer src = CreateBuffer(1024, BufferUsage.Staging);
            int[] data = Enumerable.Range(0, 256).Select(i => 2 * i).ToArray();
            GD.UpdateBuffer(src, 0, data);

            DeviceBuffer dst = CreateBuffer(1024, BufferUsage.Staging);

            CommandList copyCL = RF.CreateCommandList();
            copyCL.Begin();
            copyCL.CopyBuffer(src, 0, dst, 0, src.SizeInBytes);
            copyCL.End();
            GD.SubmitCommands(copyCL);
            GD.WaitForIdle();
            src.Dispose();

            MappedResourceView<int> view = GD.Map<int>(dst, MapMode.Read);
            for (int i = 0; i < view.Count; i++)
            {
                Assert.Equal(i * 2, view[i]);
            }
        }

        [Fact]
        public void CopyBuffer_Chain_Succeeds()
        {
            DeviceBuffer src = CreateBuffer(1024, BufferUsage.Staging);
            int[] data = Enumerable.Range(0, 256).Select(i => 2 * i).ToArray();
            GD.UpdateBuffer(src, 0, data);

            DeviceBuffer finalDst = CreateBuffer(1024, BufferUsage.Staging);

            for (int chainLength = 2; chainLength <= 10; chainLength += 4)
            {
                DeviceBuffer[] dsts = Enumerable.Range(0, chainLength)
                    .Select(i => RF.CreateBuffer(new BufferDescription(1024, BufferUsage.UniformBuffer)))
                    .ToArray();

                CommandList copyCL = RF.CreateCommandList();
                copyCL.Begin();
                copyCL.CopyBuffer(src, 0, dsts[0], 0, src.SizeInBytes);
                for (int i = 0; i < chainLength - 1; i++)
                {
                    copyCL.CopyBuffer(dsts[i], 0, dsts[i + 1], 0, src.SizeInBytes);
                }
                copyCL.CopyBuffer(dsts[dsts.Length - 1], 0, finalDst, 0, src.SizeInBytes);
                copyCL.End();
                GD.SubmitCommands(copyCL);
                GD.WaitForIdle();

                MappedResourceView<int> view = GD.Map<int>(finalDst, MapMode.Read);
                for (int i = 0; i < view.Count; i++)
                {
                    Assert.Equal(i * 2, view[i]);
                }
                GD.Unmap(finalDst);
            }
        }

        [Fact]
        public void MapThenUpdate_Fails()
        {
            if (GD.BackendType == GraphicsBackend.Vulkan)
            {
                return; // TODO
            }
            if (GD.BackendType == GraphicsBackend.Metal)
            {
                return; // TODO
            }

            DeviceBuffer buffer = RF.CreateBuffer(new BufferDescription(1024, BufferUsage.Staging));
            MappedResourceView<int> view = GD.Map<int>(buffer, MapMode.ReadWrite);
            int[] data = Enumerable.Range(0, 256).Select(i => 2 * i).ToArray();
            Assert.Throws<VeldridException>(() => GD.UpdateBuffer(buffer, 0, data));
        }

        [Fact]
        public void Map_MultipleTimes_Succeeds()
        {
            DeviceBuffer buffer = RF.CreateBuffer(new BufferDescription(1024, BufferUsage.Staging));
            MappedResource map = GD.Map(buffer, MapMode.ReadWrite);
            IntPtr dataPtr = map.Data;
            map = GD.Map(buffer, MapMode.ReadWrite);
            Assert.Equal(map.Data, dataPtr);
            map = GD.Map(buffer, MapMode.ReadWrite);
            Assert.Equal(map.Data, dataPtr);
            GD.Unmap(buffer);
            GD.Unmap(buffer);
            GD.Unmap(buffer);
        }

        [Fact]
        public void Map_DifferentMode_Fails()
        {
            if (GD.BackendType == GraphicsBackend.Vulkan)
            {
                return; // TODO
            }
            if (GD.BackendType == GraphicsBackend.Metal)
            {
                return; // TODO
            }

            DeviceBuffer buffer = RF.CreateBuffer(new BufferDescription(1024, BufferUsage.Staging));
            MappedResource map = GD.Map(buffer, MapMode.Read);
            Assert.Throws<VeldridException>(() => GD.Map(buffer, MapMode.Write));
        }

        [Fact]
        public unsafe void UnusualSize()
        {
            DeviceBuffer src = RF.CreateBuffer(
                new BufferDescription(208, BufferUsage.UniformBuffer));
            DeviceBuffer dst = RF.CreateBuffer(
                new BufferDescription(208, BufferUsage.Staging));

            byte[] data = Enumerable.Range(0, 208).Select(i => (byte)(i * 150)).ToArray();
            GD.UpdateBuffer(src, 0, data);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyBuffer(src, 0, dst, 0, src.SizeInBytes);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();
            MappedResource readMap = GD.Map(dst, MapMode.Read);
            for (int i = 0; i < readMap.SizeInBytes; i++)
            {
                Assert.Equal((byte)(i * 150), ((byte*)readMap.Data)[i]);
            }
        }

        [Fact]
        public void Update_Dynamic_NonZeroOffset()
        {
            DeviceBuffer dynamic = RF.CreateBuffer(
                new BufferDescription(1024, BufferUsage.Dynamic | BufferUsage.UniformBuffer));

            byte[] initialData = Enumerable.Range(0, 1024).Select(i => (byte)i).ToArray();
            GD.UpdateBuffer(dynamic, 0, initialData);

            byte[] replacementData = Enumerable.Repeat((byte)255, 512).ToArray();
            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.UpdateBuffer(dynamic, 512, replacementData);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            DeviceBuffer dst = RF.CreateBuffer(
                new BufferDescription(1024, BufferUsage.Staging));

            cl.Begin();
            cl.CopyBuffer(dynamic, 0, dst, 0, dynamic.SizeInBytes);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<byte> readView = GD.Map<byte>(dst, MapMode.Read);
            for (uint i = 0; i < 512; i++)
            {
                Assert.Equal((byte)i, readView[i]);
            }

            for (uint i = 512; i < 1024; i++)
            {
                Assert.Equal((byte)255, readView[i]);
            }
        }

        [Fact]
        public void Dynamic_MapRead_Fails()
        {
            DeviceBuffer dynamic = RF.CreateBuffer(
                new BufferDescription(1024, BufferUsage.Dynamic | BufferUsage.UniformBuffer));
            Assert.Throws<VeldridException>(() => GD.Map(dynamic, MapMode.Read));
            Assert.Throws<VeldridException>(() => GD.Map(dynamic, MapMode.ReadWrite));
        }

        [Fact]
        public void CommandList_Update_Staging()
        {
            DeviceBuffer staging = RF.CreateBuffer(
                new BufferDescription(1024, BufferUsage.Staging));
            byte[] data = Enumerable.Range(0, 1024).Select(i => (byte)i).ToArray();

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.UpdateBuffer(staging, 0, data);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<byte> readView = GD.Map<byte>(staging, MapMode.Read);
            for (uint i = 0; i < staging.SizeInBytes; i++)
            {
                Assert.Equal((byte)i, readView[i]);
            }
        }

        [Theory]
        [InlineData(
            60, BufferUsage.VertexBuffer, 1,
            70, BufferUsage.VertexBuffer, 13,
            11)]
        [InlineData(
            60, BufferUsage.Staging, 1,
            70, BufferUsage.VertexBuffer, 13,
            11)]
        [InlineData(
            60, BufferUsage.VertexBuffer, 1,
            70, BufferUsage.Staging, 13,
            11)]
        [InlineData(
            60, BufferUsage.Staging, 1,
            70, BufferUsage.Staging, 13,
            11)]
        [InlineData(
            5, BufferUsage.VertexBuffer, 3,
            10, BufferUsage.VertexBuffer, 7,
            2)]
        public void Copy_UnalignedRegion(
            uint srcBufferSize, BufferUsage srcUsage, uint srcCopyOffset,
            uint dstBufferSize, BufferUsage dstUsage, uint dstCopyOffset,
            uint copySize)
        {
            DeviceBuffer src = CreateBuffer(srcBufferSize, srcUsage);
            DeviceBuffer dst = CreateBuffer(dstBufferSize, dstUsage);

            byte[] data = Enumerable.Range(0, (int)srcBufferSize).Select(i => (byte)i).ToArray();
            GD.UpdateBuffer(src, 0, data);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyBuffer(src, srcCopyOffset, dst, dstCopyOffset, copySize);
            cl.End();

            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            DeviceBuffer readback = GetReadback(dst);

            MappedResourceView<byte> readView = GD.Map<byte>(readback, MapMode.Read);
            for (uint i = 0; i < copySize; i++)
            {
                byte expected = data[i + srcCopyOffset];
                byte actual = readView[i + dstCopyOffset];
                Assert.Equal(expected, actual);
            }
            GD.Unmap(readback);
        }

        [Theory]
        [InlineData(BufferUsage.VertexBuffer, 13, 5, 1)]
        [InlineData(BufferUsage.Staging, 13, 5, 1)]
        public void CommandList_UpdateNonStaging_Unaligned(BufferUsage usage, uint bufferSize, uint dataSize, uint offset)
        {
            DeviceBuffer buffer = CreateBuffer(bufferSize, usage);
            byte[] data = Enumerable.Range(0, (int)dataSize).Select(i => (byte)i).ToArray();
            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.UpdateBuffer(buffer, offset, data);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            DeviceBuffer readback = GetReadback(buffer);
            MappedResourceView<byte> readView = GD.Map<byte>(readback, MapMode.Read);
            for (uint i = 0; i < dataSize; i++)
            {
                byte expected = data[i];
                byte actual = readView[i + offset];
                Assert.Equal(expected, actual);
            }
            GD.Unmap(readback);
        }

        private DeviceBuffer CreateBuffer(uint size, BufferUsage usage)
        {
            return RF.CreateBuffer(new BufferDescription(size, usage));
        }

        private DeviceBuffer GetReadback(DeviceBuffer buffer)
        {
            DeviceBuffer readback;
            if ((buffer.Usage & BufferUsage.Staging) != 0)
            {
                readback = buffer;
            }
            else
            {
                readback = CreateBuffer(buffer.SizeInBytes, BufferUsage.Staging);
                CommandList cl = RF.CreateCommandList();
                cl.Begin();
                cl.CopyBuffer(buffer, 0, readback, 0, buffer.SizeInBytes);
                cl.End();
                GD.SubmitCommands(cl);
                GD.WaitForIdle();
            }

            return readback;
        }
    }

    public class OpenGLBufferTests : BufferTestBase<OpenGLDeviceCreator> { }
#if TEST_VULKAN
    public class VulkanBufferTests : BufferTestBase<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    public class D3D11BufferTests : BufferTestBase<D3D11DeviceCreator> { }
#endif
#if TEST_METAL
    public class MetalBufferTests : BufferTestBase<MetalDeviceCreator> { }
#endif
}
