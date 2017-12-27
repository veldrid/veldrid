using System;
using System.Linq;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class TextureTestBase<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void Map_Succeeds()
        {
            Texture texture = RF.CreateTexture(
                new TextureDescription(1024, 1024, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));

            MappedResource map = GD.Map(texture, MapMode.ReadWrite, 0);
            GD.Unmap(texture, 0);
        }

        [Fact]
        public unsafe void Update_ThenMapRead_Succeeds_R32Float()
        {
            Texture texture = RF.CreateTexture(
                new TextureDescription(1024, 1024, 1, 1, 1, PixelFormat.R32_Float, TextureUsage.Staging));

            float[] data = Enumerable.Range(0, 1024 * 1024).Select(i => (float)i).ToArray();

            fixed (float* dataPtr = data)
            {
                GD.UpdateTexture(texture, (IntPtr)dataPtr, 1024 * 1024 * 4, 0, 0, 0, 1024, 1024, 1, 0, 0);
            }

            MappedResource map = GD.Map(texture, MapMode.Read, 0);
            float* mappedFloatPtr = (float*)map.Data;

            for (int y = 0; y < 1024; y++)
            {
                for (int x = 0; x < 1024; x++)
                {
                    int index = y * 1024 + x;
                    Assert.Equal(index, mappedFloatPtr[index]);
                }
            }
        }

        [Fact]
        public unsafe void Update_ThenMapRead_Succeeds_R16UNorm()
        {
            Texture texture = RF.CreateTexture(
                new TextureDescription(1024, 1024, 1, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            ushort[] data = Enumerable.Range(0, 1024 * 1024).Select(i => (ushort)i).ToArray();

            fixed (ushort* dataPtr = data)
            {
                GD.UpdateTexture(texture, (IntPtr)dataPtr, 1024 * 1024 * sizeof(ushort), 0, 0, 0, 1024, 1024, 1, 0, 0);
            }

            MappedResource map = GD.Map(texture, MapMode.Read, 0);
            ushort* mappedFloatPtr = (ushort*)map.Data;

            for (int y = 0; y < 1024; y++)
            {
                for (int x = 0; x < 1024; x++)
                {
                    ushort index = (ushort)(y * 1024 + x);
                    Assert.Equal(index, mappedFloatPtr[index]);
                }
            }
        }

        [Fact]
        public unsafe void Update_ThenMapRead_SingleMip_Succeeds_R16UNorm()
        {
            Texture texture = RF.CreateTexture(
                new TextureDescription(1024, 1024, 1, 3, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            ushort[] data = Enumerable.Range(0, 256 * 256).Select(i => (ushort)i).ToArray();

            fixed (ushort* dataPtr = data)
            {
                GD.UpdateTexture(texture, (IntPtr)dataPtr, 256 * 256 * sizeof(ushort), 0, 0, 0, 256, 256, 1, 2, 0);
            }

            MappedResource map = GD.Map(texture, MapMode.Read, 2);
            ushort* mappedFloatPtr = (ushort*)map.Data;

            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    uint mapIndex = (uint)(y * (map.RowPitch / sizeof(ushort)) + x);
                    ushort value = (ushort)(y * 256 + x);
                    Assert.Equal(value, mappedFloatPtr[mapIndex]);
                }
            }
        }

        [Fact]
        public unsafe void Update_ThenMapRead_Mip0_Succeeds_R16UNorm()
        {
            Texture texture = RF.CreateTexture(
                new TextureDescription(256, 256, 1, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            ushort[] data = Enumerable.Range(0, 256 * 256).Select(i => (ushort)i).ToArray();

            fixed (ushort* dataPtr = data)
            {
                GD.UpdateTexture(texture, (IntPtr)dataPtr, 256 * 256 * sizeof(ushort), 0, 0, 0, 256, 256, 1, 0, 0);
            }

            MappedResource map = GD.Map(texture, MapMode.Read, 0);
            ushort* mappedFloatPtr = (ushort*)map.Data;

            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    ushort index = (ushort)(y * 256 + x);
                    Assert.Equal(index, mappedFloatPtr[index]);
                }
            }
        }

        [Fact]
        public unsafe void Update_ThenCopySingleMip_Succeeds_R16UNorm()
        {
            TextureDescription desc = new TextureDescription(
                1024, 1024, 1, 3, 1, PixelFormat.R16_UNorm, TextureUsage.Staging);
            Texture src = RF.CreateTexture(desc);
            Texture dst = RF.CreateTexture(desc);

            ushort[] data = Enumerable.Range(0, 256 * 256).Select(i => (ushort)i).ToArray();

            fixed (ushort* dataPtr = data)
            {
                GD.UpdateTexture(src, (IntPtr)dataPtr, 256 * 256 * sizeof(ushort), 0, 0, 0, 256, 256, 1, 2, 0);
            }

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(src, dst, 2, 0);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResource map = GD.Map(dst, MapMode.Read, 2);
            ushort* mappedFloatPtr = (ushort*)map.Data;

            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    uint mapIndex = (uint)(y * (map.RowPitch / sizeof(ushort)) + x);
                    ushort value = (ushort)(y * 256 + x);
                    Assert.Equal(value, mappedFloatPtr[mapIndex]);
                }
            }
        }

        [Fact]
        public unsafe void Copy_BC3_Unorm()
        {
            Texture copySrc = RF.CreateTexture(new TextureDescription(
                64, 64, 1, 1, 1, PixelFormat.BC3_UNorm, TextureUsage.Staging));
            Texture copyDst = RF.CreateTexture(new TextureDescription(
                64, 64, 1, 1, 1, PixelFormat.BC3_UNorm, TextureUsage.Staging));

            uint totalDataSize = copySrc.Width * copySrc.Height;
            byte[] data = new byte[totalDataSize];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }
            fixed (byte* dataPtr = data)
            {
                GD.UpdateTexture(copySrc, (IntPtr)dataPtr, totalDataSize, 0, 0, 0, copySrc.Width, copySrc.Height, 1, 0, 0);
            }

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(
                copySrc, 0, 0, 0, 0, 0,
                copyDst, 0, 0, 0, 0, 0,
                copySrc.Width, copySrc.Height, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();
            MappedResourceView<byte> view = GD.Map<byte>(copyDst, MapMode.Read);
            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(view[i], data[i]);
            }
        }
    }

#if TEST_VULKAN
    public class VulkanTextureTests : TextureTestBase<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    public class D3D11TextureTests : TextureTestBase<D3D11DeviceCreator> { }
#endif
    public class OpenGLTextureTests : TextureTestBase<OpenGLDeviceCreator> { }
}
