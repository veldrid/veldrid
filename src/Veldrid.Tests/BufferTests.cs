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

            Buffer buffer = RF.CreateBuffer(new BufferDescription(expectedSize, expectedUsage));

            Assert.Equal(expectedUsage, buffer.Usage);
            Assert.Equal(expectedSize, buffer.SizeInBytes);
        }

        [Fact]
        public void UpdateBuffer_NonDynamic_Succeeds()
        {
            Buffer buffer = CreateBuffer(64, BufferUsage.VertexBuffer);
            GD.UpdateBuffer(buffer, 0, Matrix4x4.Identity);
            GD.WaitForIdle();
        }

        [Fact]
        public void UpdateBuffer_ThenMapRead_Succeeds()
        {
            Buffer buffer = CreateBuffer(1024, BufferUsage.Staging);
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
            Buffer buffer = CreateBuffer(256, BufferUsage.Staging);
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
            Buffer buffer = CreateBuffer(1024, BufferUsage.Staging);
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
        }

        [Fact]
        public void MapGeneric_OutOfBounds_ThrowsIndexOutOfRange()
        {
            Buffer buffer = CreateBuffer(1024, BufferUsage.Staging);
            MappedResourceView<byte> view = GD.Map<byte>(buffer, MapMode.ReadWrite);
            Assert.Throws<IndexOutOfRangeException>(() => view[1024]);
            Assert.Throws<IndexOutOfRangeException>(() => view[-1]);
        }

        [Fact]
        public void Map_WrongFlags_Throws()
        {
            Buffer buffer = CreateBuffer(1024, BufferUsage.VertexBuffer);
            Assert.Throws<VeldridException>(() => GD.Map(buffer, MapMode.Read));
            Assert.Throws<VeldridException>(() => GD.Map(buffer, MapMode.Write));
            Assert.Throws<VeldridException>(() => GD.Map(buffer, MapMode.ReadWrite));
        }

        [Fact]
        public void CopyBuffer_Succeeds()
        {
            Buffer src = CreateBuffer(1024, BufferUsage.Staging);
            int[] data = Enumerable.Range(0, 256).Select(i => 2 * i).ToArray();
            GD.UpdateBuffer(src, 0, data);

            Buffer dst = CreateBuffer(1024, BufferUsage.Staging);

            CommandList copyCL = RF.CreateCommandList();
            copyCL.Begin();
            copyCL.CopyBuffer(src, 0, dst, 0, src.SizeInBytes);
            copyCL.End();
            GD.ExecuteCommands(copyCL);
            src.Dispose();
            GD.WaitForIdle();

            MappedResourceView<int> view = GD.Map<int>(dst, MapMode.Read);
            for (int i = 0; i < view.Count; i++)
            {
                Assert.Equal(i * 2, view[i]);
            }
        }

        [Fact]
        public void CopyBuffer_Chain_Succeeds()
        {
            Buffer src = CreateBuffer(1024, BufferUsage.Staging);
            int[] data = Enumerable.Range(0, 256).Select(i => 2 * i).ToArray();
            GD.UpdateBuffer(src, 0, data);

            Buffer finalDst = CreateBuffer(1024, BufferUsage.Staging);

            for (int chainLength = 2; chainLength <= 10; chainLength += 4)
            {
                Buffer[] dsts = Enumerable.Range(0, chainLength)
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
                GD.ExecuteCommands(copyCL);
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

            Buffer buffer = RF.CreateBuffer(new BufferDescription(1024, BufferUsage.Staging));
            MappedResourceView<int> view = GD.Map<int>(buffer, MapMode.ReadWrite);
            int[] data = Enumerable.Range(0, 256).Select(i => 2 * i).ToArray();
            Assert.Throws<VeldridException>(() => GD.UpdateBuffer(buffer, 0, data));
        }

        [Fact]
        public void Map_MultipleTimes_Succeeds()
        {
            Buffer buffer = RF.CreateBuffer(new BufferDescription(1024, BufferUsage.Staging));
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

            Buffer buffer = RF.CreateBuffer(new BufferDescription(1024, BufferUsage.Staging));
            MappedResource map = GD.Map(buffer, MapMode.Read);
            Assert.Throws<VeldridException>(() => GD.Map(buffer, MapMode.Write));
        }

        private Buffer CreateBuffer(uint size, BufferUsage usage)
        {
            return RF.CreateBuffer(new BufferDescription(size, usage));
        }
    }

    public class VulkanBufferTests : BufferTestBase<VulkanDeviceCreator> { }
    public class OpenGLBufferTests : BufferTestBase<OpenGLDeviceCreator> { }
#if TEST_D3D11
    public class D3D11BufferTests : BufferTestBase<D3D11DeviceCreator> { }
#endif
}
