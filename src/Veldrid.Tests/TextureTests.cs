using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class TextureTestBase<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void Map_Succeeds()
        {
            Texture texture = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));

            MappedResource map = GD.Map(texture, MapMode.ReadWrite, 0);
            GD.Unmap(texture, 0);
        }

        [Fact]
        public void Map_Succeeds_R32_G32_B32_A32_UInt()
        {
            Texture texture = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 1, 1, PixelFormat.R32_G32_B32_A32_UInt, TextureUsage.Staging));

            MappedResource map = GD.Map(texture, MapMode.ReadWrite, 0);
            GD.Unmap(texture, 0);
        }

        [Fact]
        public unsafe void Update_ThenMapRead_Succeeds_R32Float()
        {
            Texture texture = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 1, 1, PixelFormat.R32_Float, TextureUsage.Staging));

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
                TextureDescription.Texture2D(1024, 1024, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

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
        public unsafe void Update_ThenMapRead_Succeeds_R8_G8_SNorm()
        {
            Texture texture = RF.CreateTexture(
                TextureDescription.Texture2D(8, 8, 1, 1, PixelFormat.R8_G8_SNorm, TextureUsage.Staging));

            byte[] data = Enumerable.Range(0, 8 * 8 * 2).Select(i => (byte)i).ToArray();

            fixed (byte* dataPtr = data)
            {
                GD.UpdateTexture(texture, (IntPtr)dataPtr, 8 * 8 * sizeof(byte) * 2, 0, 0, 0, 8, 8, 1, 0, 0);
            }

            MappedResource map = GD.Map(texture, MapMode.Read, 0);
            byte* mappedFloatPtr = (byte*)map.Data;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    uint index0 = (uint)(y * map.RowPitch + x * 2);
                    byte value0 = (byte)(y * 8 * 2 + x * 2);
                    Assert.Equal(value0, mappedFloatPtr[index0]);

                    uint index1 = (uint)(index0 + 1);
                    byte value1 = (byte)(value0 + 1);
                    Assert.Equal(value1, mappedFloatPtr[index1]);
                }
            }
        }

        [Fact]
        public unsafe void Update_ThenMapRead_SingleMip_Succeeds_R16UNorm()
        {
            Texture texture = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 3, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

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
                TextureDescription.Texture2D(256, 256, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

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
            TextureDescription desc = TextureDescription.Texture2D(
                1024, 1024, 3, 1, PixelFormat.R16_UNorm, TextureUsage.Staging);
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
            Texture copySrc = RF.CreateTexture(TextureDescription.Texture2D(
                64, 64, 1, 1, PixelFormat.BC3_UNorm, TextureUsage.Staging));
            Texture copyDst = RF.CreateTexture(TextureDescription.Texture2D(
                64, 64, 1, 1, PixelFormat.BC3_UNorm, TextureUsage.Staging));

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

        [Fact]
        public unsafe void Update_ThenMapRead_3D()
        {
            Texture tex3D = RF.CreateTexture(TextureDescription.Texture3D(
                10, 10, 10, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            RgbaByte[] data = new RgbaByte[tex3D.Width * tex3D.Height * tex3D.Depth];
            for (int z = 0; z < tex3D.Depth; z++)
                for (int y = 0; y < tex3D.Height; y++)
                    for (int x = 0; x < tex3D.Width; x++)
                    {
                        int index = (int)(z * tex3D.Width * tex3D.Height + y * tex3D.Height + x);
                        data[index] = new RgbaByte((byte)x, (byte)y, (byte)z, 1);
                    }

            fixed (RgbaByte* dataPtr = data)
            {
                GD.UpdateTexture(tex3D, (IntPtr)dataPtr, (uint)(data.Length * Unsafe.SizeOf<RgbaByte>()),
                    0, 0, 0,
                    tex3D.Width, tex3D.Height, tex3D.Depth,
                    0, 0);
            }

            MappedResourceView<RgbaByte> view = GD.Map<RgbaByte>(tex3D, MapMode.Read, 0);
            for (int z = 0; z < tex3D.Depth; z++)
                for (int y = 0; y < tex3D.Height; y++)
                    for (int x = 0; x < tex3D.Width; x++)
                    {
                        Assert.Equal(new RgbaByte((byte)x, (byte)y, (byte)z, 1), view[x, y, z]);
                    }
            GD.Unmap(tex3D);
        }

        [Fact]
        public unsafe void MapWrite_ThenMapRead_3D()
        {
            Texture tex3D = RF.CreateTexture(TextureDescription.Texture3D(
                10, 10, 10, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            MappedResourceView<RgbaByte> writeView = GD.Map<RgbaByte>(tex3D, MapMode.Write);
            for (int z = 0; z < tex3D.Depth; z++)
                for (int y = 0; y < tex3D.Height; y++)
                    for (int x = 0; x < tex3D.Width; x++)
                    {
                        writeView[x, y, z] = new RgbaByte((byte)x, (byte)y, (byte)z, 1);
                    }
            GD.Unmap(tex3D);

            MappedResourceView<RgbaByte> readView = GD.Map<RgbaByte>(tex3D, MapMode.Read, 0);
            for (int z = 0; z < tex3D.Depth; z++)
                for (int y = 0; y < tex3D.Height; y++)
                    for (int x = 0; x < tex3D.Width; x++)
                    {
                        Assert.Equal(new RgbaByte((byte)x, (byte)y, (byte)z, 1), readView[x, y, z]);
                    }
            GD.Unmap(tex3D);
        }

        [Fact]
        public unsafe void Update_ThenMapRead_1D()
        {
            Texture tex1D = RF.CreateTexture(
                TextureDescription.Texture1D(100, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));
            ushort[] data = Enumerable.Range(0, (int)tex1D.Width).Select(i => (ushort)(i * 2)).ToArray();
            fixed (ushort* dataPtr = &data[0])
            {
                GD.UpdateTexture(tex1D, (IntPtr)dataPtr, (uint)(data.Length * sizeof(ushort)), 0, 0, 0, tex1D.Width, 1, 1, 0, 0);
            }

            MappedResourceView<ushort> view = GD.Map<ushort>(tex1D, MapMode.Read);
            for (int i = 0; i < view.Count; i++)
            {
                Assert.Equal((ushort)(i * 2), view[i]);
            }
            GD.Unmap(tex1D);
        }

        [Fact]
        public unsafe void MapWrite_ThenMapRead_1D()
        {
            Texture tex1D = RF.CreateTexture(
                TextureDescription.Texture1D(100, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            MappedResourceView<ushort> writeView = GD.Map<ushort>(tex1D, MapMode.Write);
            Assert.Equal(tex1D.Width, (uint)writeView.Count);
            for (int i = 0; i < writeView.Count; i++)
            {
                writeView[i] = (ushort)(i * 2);
            }
            GD.Unmap(tex1D);

            MappedResourceView<ushort> view = GD.Map<ushort>(tex1D, MapMode.Read);
            for (int i = 0; i < view.Count; i++)
            {
                Assert.Equal((ushort)(i * 2), view[i]);
            }
            GD.Unmap(tex1D);
        }

        [Fact]
        public unsafe void Copy_1DTo2D()
        {
            Texture tex1D = RF.CreateTexture(
                TextureDescription.Texture1D(100, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));
            Texture tex2D = RF.CreateTexture(
                TextureDescription.Texture2D(100, 10, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            MappedResourceView<ushort> writeView = GD.Map<ushort>(tex1D, MapMode.Write);
            Assert.Equal(tex1D.Width, (uint)writeView.Count);
            for (int i = 0; i < writeView.Count; i++)
            {
                writeView[i] = (ushort)(i * 2);
            }
            GD.Unmap(tex1D);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(
                tex1D, 0, 0, 0, 0, 0,
                tex2D, 0, 5, 0, 0, 0,
                tex1D.Width, 1, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.DisposeWhenIdle(cl);
            GD.WaitForIdle();

            MappedResourceView<ushort> readView = GD.Map<ushort>(tex2D, MapMode.Read);
            for (int i = 0; i < tex2D.Width; i++)
            {
                Assert.Equal((ushort)(i * 2), readView[i, 5]);
            }
            GD.Unmap(tex2D);
        }

        [Fact]
        public void Update_MultipleMips_1D()
        {
            Texture tex1D = RF.CreateTexture(TextureDescription.Texture1D(
                100, 5, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            for (uint level = 0; level < tex1D.MipLevels; level++)
            {
                MappedResourceView<RgbaByte> writeView = GD.Map<RgbaByte>(tex1D, MapMode.Write, level);
                for (int i = 0; i < writeView.Count; i++)
                {
                    writeView[i] = new RgbaByte((byte)i, (byte)(i * 2), (byte)level, 1);
                }
                GD.Unmap(tex1D, level);
            }

            for (uint level = 0; level < tex1D.MipLevels; level++)
            {
                MappedResourceView<RgbaByte> readView = GD.Map<RgbaByte>(tex1D, MapMode.Read, level);
                for (int i = 0; i < readView.Count; i++)
                {
                    Assert.Equal(new RgbaByte((byte)i, (byte)(i * 2), (byte)level, 1), readView[i]);
                }
                GD.Unmap(tex1D, level);
            }
        }

        [Fact]
        public void Copy_DifferentMip_1DTo2D()
        {
            Texture tex1D = RF.CreateTexture(
                TextureDescription.Texture1D(200, 2, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));
            Texture tex2D = RF.CreateTexture(
                TextureDescription.Texture2D(100, 10, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            MappedResourceView<ushort> writeView = GD.Map<ushort>(tex1D, MapMode.Write, 1);
            Assert.Equal(tex2D.Width, (uint)writeView.Count);
            for (int i = 0; i < writeView.Count; i++)
            {
                writeView[i] = (ushort)(i * 2);
            }
            GD.Unmap(tex1D, 1);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(
                tex1D, 0, 0, 0, 1, 0,
                tex2D, 0, 5, 0, 0, 0,
                tex2D.Width, 1, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.DisposeWhenIdle(cl);
            GD.WaitForIdle();

            MappedResourceView<ushort> readView = GD.Map<ushort>(tex2D, MapMode.Read);
            for (int i = 0; i < tex2D.Width; i++)
            {
                Assert.Equal((ushort)(i * 2), readView[i, 5]);
            }
            GD.Unmap(tex2D);
        }

        [Fact]
        public void Copy_WitOffsets_2D()
        {
            Texture src = RF.CreateTexture(TextureDescription.Texture2D(
                100, 100, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            Texture dst = RF.CreateTexture(TextureDescription.Texture2D(
                100, 100, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            MappedResourceView<RgbaByte> writeView = GD.Map<RgbaByte>(src, MapMode.Write);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                {
                    writeView[x, y] = new RgbaByte((byte)x, (byte)y, 0, 1);
                }
            GD.Unmap(src);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(
                src,
                50, 50, 0, 0, 0,
                dst, 10, 10, 0, 0, 0,
                50, 50, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaByte> readView = GD.Map<RgbaByte>(dst, MapMode.Read);
            for (int y = 10; y < 60; y++)
                for (int x = 10; x < 60; x++)
                {
                    Assert.Equal(new RgbaByte((byte)(x + 40), (byte)(y + 40), 0, 1), readView[x, y]);
                }
            GD.Unmap(dst);
        }

        [Fact]
        public void Copy_ArrayToNonArray()
        {
            Texture src = RF.CreateTexture(TextureDescription.Texture2D(
                10, 10, 1, 10, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));
            Texture dst = RF.CreateTexture(TextureDescription.Texture2D(
                10, 10, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            MappedResourceView<RgbaByte> writeView = GD.Map<RgbaByte>(src, MapMode.Write, 5);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                {
                    writeView[x, y] = new RgbaByte((byte)x, (byte)y, 0, 1);
                }
            GD.Unmap(src, 5);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(
                src, 0, 0, 0, 0, 5,
                dst, 0, 0, 0, 0, 0,
                10, 10, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaByte> readView = GD.Map<RgbaByte>(dst, MapMode.Read);
            for (int y = 0; y < dst.Height; y++)
                for (int x = 0; x < dst.Width; x++)
                {
                    Assert.Equal(new RgbaByte((byte)x, (byte)y, 0, 1), readView[x, y]);
                }
            GD.Unmap(dst);
        }

        [Fact]
        public void Map_ThenRead_MultipleArrayLayers()
        {
            Texture src = RF.CreateTexture(TextureDescription.Texture2D(
                10, 10, 1, 10, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            for (uint layer = 0; layer < src.ArrayLayers; layer++)
            {
                MappedResourceView<RgbaByte> writeView = GD.Map<RgbaByte>(src, MapMode.Write, layer);
                for (int y = 0; y < src.Height; y++)
                    for (int x = 0; x < src.Width; x++)
                    {
                        writeView[x, y] = new RgbaByte((byte)x, (byte)y, (byte)layer, 1);
                    }
                GD.Unmap(src, layer);
            }

            for (uint layer = 0; layer < src.ArrayLayers; layer++)
            {
                MappedResourceView<RgbaByte> readView = GD.Map<RgbaByte>(src, MapMode.Read, layer);
                for (int y = 0; y < src.Height; y++)
                    for (int x = 0; x < src.Width; x++)
                    {
                        Assert.Equal(new RgbaByte((byte)x, (byte)y, (byte)layer, 1), readView[x, y]);
                    }
                GD.Unmap(src, layer);
            }
        }

        [Fact]
        public unsafe void Update_WithOffset_2D()
        {
            Texture tex2D = RF.CreateTexture(TextureDescription.Texture2D(
                100, 100, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            RgbaByte[] data = new RgbaByte[50 * 30];
            for (uint y = 0; y < 30; y++)
                for (uint x = 0; x < 50; x++)
                {
                    data[y * 50 + x] = new RgbaByte((byte)x, (byte)y, 0, 1);
                }

            fixed (RgbaByte* dataPtr = &data[0])
            {
                GD.UpdateTexture(
                    tex2D, (IntPtr)dataPtr, (uint)(data.Length * sizeof(RgbaByte)),
                    50, 70, 0,
                    50, 30, 1,
                    0, 0);
            }

            MappedResourceView<RgbaByte> readView = GD.Map<RgbaByte>(tex2D, MapMode.Read);
            for (int y = 0; y < 30; y++)
                for (int x = 0; x < 50; x++)
                {
                    Assert.Equal(new RgbaByte((byte)x, (byte)y, 0, 1), readView[x + 50, y + 70]);
                }
        }

        [Fact]
        public unsafe void Map_NonZeroMip_3D()
        {
            Texture tex3D = RF.CreateTexture(TextureDescription.Texture3D(
                40, 40, 40, 3, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            MappedResourceView<RgbaByte> writeView = GD.Map<RgbaByte>(tex3D, MapMode.Write, 2);
            for (int z = 0; z < 10; z++)
                for (int y = 0; y < 10; y++)
                    for (int x = 0; x < 10; x++)
                    {
                        writeView[x, y, z] = new RgbaByte((byte)x, (byte)y, (byte)z, 1);
                    }
            GD.Unmap(tex3D, 2);

            MappedResourceView<RgbaByte> readView = GD.Map<RgbaByte>(tex3D, MapMode.Read, 2);
            for (int z = 0; z < 10; z++)
                for (int y = 0; y < 10; y++)
                    for (int x = 0; x < 10; x++)
                    {
                        Assert.Equal(new RgbaByte((byte)x, (byte)y, (byte)z, 1), readView[x, y, z]);
                    }
            GD.Unmap(tex3D, 2);
        }

        [Fact]
        public unsafe void Update_NonStaging_3D()
        {
            Texture tex3D = RF.CreateTexture(TextureDescription.Texture3D(
                16, 16, 16, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            RgbaByte[] data = new RgbaByte[16 * 16 * 16];
            for (int z = 0; z < 16; z++)
                for (int y = 0; y < 16; y++)
                    for (int x = 0; x < 16; x++)
                    {
                        int index = (int)(z * tex3D.Width * tex3D.Height + y * tex3D.Height + x);
                        data[index] = new RgbaByte((byte)x, (byte)y, (byte)z, 1);
                    }

            fixed (RgbaByte* dataPtr = data)
            {
                GD.UpdateTexture(tex3D, (IntPtr)dataPtr, (uint)(data.Length * Unsafe.SizeOf<RgbaByte>()),
                    0, 0, 0,
                    tex3D.Width, tex3D.Height, tex3D.Depth,
                    0, 0);
            }

            Texture staging = RF.CreateTexture(TextureDescription.Texture3D(
                16, 16, 16, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(tex3D, staging);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<RgbaByte> view = GD.Map<RgbaByte>(staging, MapMode.Read);
            for (int z = 0; z < tex3D.Depth; z++)
                for (int y = 0; y < tex3D.Height; y++)
                    for (int x = 0; x < tex3D.Width; x++)
                    {
                        Assert.Equal(new RgbaByte((byte)x, (byte)y, (byte)z, 1), view[x, y, z]);
                    }
            GD.Unmap(staging);
        }

        [Fact]
        public unsafe void Copy_NonSquareTexture()
        {
            Texture src = RF.CreateTexture(
                TextureDescription.Texture2D(512, 128, 1, 1, PixelFormat.R8_UNorm, TextureUsage.Staging));
            byte[] data = Enumerable.Repeat((byte)255, (int)(src.Width * src.Height)).ToArray();
            fixed (byte* dataPtr = data)
            {
                GD.UpdateTexture(src, (IntPtr)dataPtr, (uint)data.Length,
                    0, 0, 0,
                    src.Width, src.Height, 1,
                    0, 0);
            }

            Texture dst = RF.CreateTexture(
                TextureDescription.Texture2D(512, 128, 1, 1, PixelFormat.R8_UNorm, TextureUsage.Staging));
            byte[] data2 = Enumerable.Repeat((byte)100, (int)(dst.Width * dst.Height)).ToArray();
            fixed (byte* dataPtr2 = data2)
            {
                GD.UpdateTexture(dst, (IntPtr)dataPtr2, (uint)data2.Length,
                    0, 0, 0,
                    dst.Width, dst.Height, 1,
                    0, 0);
            }

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(src, dst);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResourceView<byte> readView = GD.Map<byte>(dst, MapMode.Read);
            for (uint y = 0; y < dst.Height; y++)
                for (uint x = 0; x < dst.Width; x++)
                {
                    Assert.Equal(255, readView[x, y]);
                }

            GD.Unmap(dst);
        }
    }

#if TEST_VULKAN
    public class VulkanTextureTests : TextureTestBase<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    public class D3D11TextureTests : TextureTestBase<D3D11DeviceCreator> { }
#endif
#if TEST_METAL
    public class MetalTextureTests : TextureTestBase<MetalDeviceCreator> { }
#endif
    public class OpenGLTextureTests : TextureTestBase<OpenGLDeviceCreator> { }
}
