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
                GD.UpdateTexture(texture, (IntPtr)dataPtr, 1024 * 1024 * 4, 0, 0, 0, 1024, 1024, 1, 0, 0);
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
    }

    public class VulkanTextureTests : TextureTestBase<VulkanDeviceCreator> { }
#if TEST_D3D11
    public class D3D11TextureTests : TextureTestBase<D3D11DeviceCreator> { }
#endif
    public class OpenGLTextureTests : TextureTestBase<OpenGLDeviceCreator> { }
}
