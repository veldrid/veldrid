using System;
using System.Collections.Generic;
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public unsafe void Update_ThenMapRead_Succeeds_R32Float(bool useArrayOverload)
        {
            Texture texture = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 1, 1, PixelFormat.R32_Float, TextureUsage.Staging));

            float[] data = Enumerable.Range(0, 1024 * 1024).Select(i => (float)i).ToArray();

            fixed (float* dataPtr = data)
            {
                if (useArrayOverload)
                {
                    GD.UpdateTexture(texture, data, 0, 0, 0, 1024, 1024, 1, 0, 0);
                }
                else
                {
                    GD.UpdateTexture(texture, (IntPtr)dataPtr, 1024 * 1024 * 4, 0, 0, 0, 1024, 1024, 1, 0, 0);
                }
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public unsafe void Update_ThenMapRead_SingleMip_Succeeds_R16UNorm(bool useArrayOverload)
        {
            Texture texture = RF.CreateTexture(
                TextureDescription.Texture2D(1024, 1024, 3, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            ushort[] data = Enumerable.Range(0, 256 * 256).Select(i => (ushort)i).ToArray();

            fixed (ushort* dataPtr = data)
            {
                if (useArrayOverload)
                {
                    GD.UpdateTexture(texture, data, 0, 0, 0, 256, 256, 1, 2, 0);
                }
                else
                {
                    GD.UpdateTexture(texture, (IntPtr)dataPtr, 256 * 256 * sizeof(ushort), 0, 0, 0, 256, 256, 1, 2, 0);
                }
            }

            MappedResource map = GD.Map(texture, MapMode.Read, 2);
            ushort* mappedUShortPtr = (ushort*)map.Data;

            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    uint mapIndex = (uint)(y * (map.RowPitch / sizeof(ushort)) + x);
                    ushort value = (ushort)(y * 256 + x);
                    Assert.Equal(value, mappedUShortPtr[mapIndex]);
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

        [Theory]
        [InlineData(PixelFormat.BC1_Rgb_UNorm, 8, 0, 0, 64, 64)]
        [InlineData(PixelFormat.BC1_Rgb_UNorm, 8, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC1_Rgb_UNorm_SRgb, 8, 0, 0, 64, 64)]
        [InlineData(PixelFormat.BC1_Rgb_UNorm_SRgb, 8, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC1_Rgba_UNorm, 8, 0, 0, 64, 64)]
        [InlineData(PixelFormat.BC1_Rgba_UNorm, 8, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC1_Rgba_UNorm_SRgb, 8, 0, 0, 64, 64)]
        [InlineData(PixelFormat.BC1_Rgba_UNorm_SRgb, 8, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC2_UNorm, 16, 0, 0, 64, 64)]
        [InlineData(PixelFormat.BC2_UNorm, 16, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC2_UNorm_SRgb, 16, 0, 0, 64, 64)]
        [InlineData(PixelFormat.BC2_UNorm_SRgb, 16, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC3_UNorm, 16, 0, 0, 64, 64)]
        [InlineData(PixelFormat.BC3_UNorm, 16, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC3_UNorm_SRgb, 16, 0, 0, 64, 64)]
        [InlineData(PixelFormat.BC3_UNorm_SRgb, 16, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC4_UNorm, 8, 0, 0, 16, 16)]
        [InlineData(PixelFormat.BC4_UNorm, 8, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC4_SNorm, 8, 0, 0, 16, 16)]
        [InlineData(PixelFormat.BC4_SNorm, 8, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC5_UNorm, 16, 0, 0, 16, 16)]
        [InlineData(PixelFormat.BC5_UNorm, 16, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC5_SNorm, 16, 0, 0, 16, 16)]
        [InlineData(PixelFormat.BC5_SNorm, 16, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC7_UNorm, 16, 0, 0, 16, 16)]
        [InlineData(PixelFormat.BC7_UNorm, 16, 8, 4, 16, 16)]
        [InlineData(PixelFormat.BC7_UNorm_SRgb, 16, 0, 0, 16, 16)]
        [InlineData(PixelFormat.BC7_UNorm_SRgb, 16, 8, 4, 16, 16)]
        public unsafe void Copy_Compressed_Texture(PixelFormat format, uint blockSizeInBytes, uint srcX, uint srcY, uint copyWidth, uint copyHeight)
        {
            if (!GD.GetPixelFormatSupport(format, TextureType.Texture2D, TextureUsage.Sampled))
            {
                return;
            }

            Texture copySrc = RF.CreateTexture(TextureDescription.Texture2D(
                64, 64, 1, 1, format, TextureUsage.Staging));
            Texture copyDst = RF.CreateTexture(TextureDescription.Texture2D(
                copyWidth, copyHeight, 1, 1, format, TextureUsage.Staging));

            const int numPixelsInBlockSide = 4;
            const int numPixelsInBlock = 16;

            uint totalDataSize = copyWidth * copyHeight / numPixelsInBlock * blockSizeInBytes;
            byte[] data = new byte[totalDataSize];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }
            fixed (byte* dataPtr = data)
            {
                GD.UpdateTexture(copySrc, (IntPtr)dataPtr, totalDataSize, srcX, srcY, 0, copyWidth, copyHeight, 1, 0, 0);
            }

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(
                copySrc, srcX, srcY, 0, 0, 0,
                copyDst, 0, 0, 0, 0, 0,
                copyWidth, copyHeight, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            uint numBytesPerRow = copyWidth / numPixelsInBlockSide * blockSizeInBytes;
            MappedResourceView<byte> view = GD.Map<byte>(copyDst, MapMode.Read);
            for (uint i = 0; i < data.Length; i++)
            {
                uint viewRow = i / numBytesPerRow;
                uint viewIndex = (view.MappedResource.RowPitch * viewRow) + (i % numBytesPerRow);
                Assert.Equal(data[i], view[viewIndex]);
            }
            GD.Unmap(copyDst);
        }

        // [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public unsafe void Copy_Compressed_Array(bool separateLayerCopies)
        {
            PixelFormat format = PixelFormat.BC3_UNorm;
            if (!GD.GetPixelFormatSupport(format, TextureType.Texture2D, TextureUsage.Sampled))
            {
                return;
            }

            TextureDescription texDesc = TextureDescription.Texture2D(
                16, 16,
                1, 4,
                format,
                TextureUsage.Sampled);

            Texture copySrc = RF.CreateTexture(texDesc);
            texDesc.Usage = TextureUsage.Staging;
            Texture copyDst = RF.CreateTexture(texDesc);

            for (uint layer = 0; layer < copySrc.ArrayLayers; layer++)
            {
                int byteCount = 16 * 16;
                byte[] data = Enumerable.Range(0, byteCount).Select(i => (byte)(i + layer)).ToArray();
                GD.UpdateTexture(
                    copySrc,
                    data,
                    0, 0, 0,
                    16, 16, 1,
                    0, layer);
            }

            CommandList copyCL = RF.CreateCommandList();
            copyCL.Begin();
            if (separateLayerCopies)
            {
                for (uint layer = 0; layer < copySrc.ArrayLayers; layer++)
                {
                    copyCL.CopyTexture(copySrc, 0, 0, 0, 0, layer, copyDst, 0, 0, 0, 0, layer, 16, 16, 1, 1);
                }
            }
            else
            {
                copyCL.CopyTexture(copySrc, 0, 0, 0, 0, 0, copyDst, 0, 0, 0, 0, 0, 16, 16, 1, copySrc.ArrayLayers);
            }
            copyCL.End();
            Fence fence = RF.CreateFence(false);
            GD.SubmitCommands(copyCL, fence);
            GD.WaitForFence(fence);

            for (uint layer = 0; layer < copyDst.ArrayLayers; layer++)
            {
                MappedResource map = GD.Map(copyDst, MapMode.Read, layer);
                byte* basePtr = (byte*)map.Data;

                int index = 0;
                uint rowSize = 64;
                uint numRows = 4;
                for (uint row = 0; row < numRows; row++)
                {
                    byte* rowBase = basePtr + (row * map.RowPitch);
                    for (uint x = 0; x < rowSize; x++)
                    {
                        Assert.Equal((byte)(index + layer), rowBase[x]);
                        index += 1;
                    }
                }

                GD.Unmap(copyDst, layer);
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
            if (!GD.Features.Texture1D) { return; }

            Texture tex1D = RF.CreateTexture(
                TextureDescription.Texture1D(100, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));
            ushort[] data = Enumerable.Range(0, (int)tex1D.Width).Select(i => (ushort)(i * 2)).ToArray();
            fixed (ushort* dataPtr = &data[0])
            {
                GD.UpdateTexture(tex1D, (IntPtr)dataPtr, (uint)(data.Length * sizeof(ushort)), 0, 0, 0, tex1D.Width, 1, 1, 0, 0);
            }

            MappedResourceView<ushort> view = GD.Map<ushort>(tex1D, MapMode.Read);
            for (int i = 0; i < tex1D.Width; i++)
            {
                Assert.Equal((ushort)(i * 2), view[i]);
            }
            GD.Unmap(tex1D);
        }

        [Fact]
        public unsafe void MapWrite_ThenMapRead_1D()
        {
            if (!GD.Features.Texture1D) { return; }

            Texture tex1D = RF.CreateTexture(
                TextureDescription.Texture1D(100, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            MappedResourceView<ushort> writeView = GD.Map<ushort>(tex1D, MapMode.Write);
            for (int i = 0; i < tex1D.Width; i++)
            {
                writeView[i] = (ushort)(i * 2);
            }
            GD.Unmap(tex1D);

            MappedResourceView<ushort> view = GD.Map<ushort>(tex1D, MapMode.Read);
            for (int i = 0; i < tex1D.Width; i++)
            {
                Assert.Equal((ushort)(i * 2), view[i]);
            }
            GD.Unmap(tex1D);
        }

        [Fact]
        public unsafe void Copy_1DTo2D()
        {
            if (!GD.Features.Texture1D) { return; }

            Texture tex1D = RF.CreateTexture(
                TextureDescription.Texture1D(100, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));
            Texture tex2D = RF.CreateTexture(
                TextureDescription.Texture2D(100, 10, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            MappedResourceView<ushort> writeView = GD.Map<ushort>(tex1D, MapMode.Write);
            for (int i = 0; i < tex1D.Width; i++)
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
            cl.Dispose();
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
            if (!GD.Features.Texture1D) { return; }

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
            if (!GD.Features.Texture1D) { return; }

            Texture tex1D = RF.CreateTexture(
                TextureDescription.Texture1D(200, 2, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));
            Texture tex2D = RF.CreateTexture(
                TextureDescription.Texture2D(100, 10, 1, 1, PixelFormat.R16_UNorm, TextureUsage.Staging));

            MappedResourceView<ushort> writeView = GD.Map<ushort>(tex1D, MapMode.Write, 1);
            for (int i = 0; i < tex2D.Width; i++)
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
            cl.Dispose();
            GD.WaitForIdle();

            MappedResourceView<ushort> readView = GD.Map<ushort>(tex2D, MapMode.Read);
            for (int i = 0; i < tex2D.Width; i++)
            {
                Assert.Equal((ushort)(i * 2), readView[i, 5]);
            }
            GD.Unmap(tex2D);
        }

        [InlineData(TextureUsage.Staging, TextureUsage.Staging)]
        [InlineData(TextureUsage.Staging, TextureUsage.Sampled)]
        [InlineData(TextureUsage.Sampled, TextureUsage.Staging)]
        [InlineData(TextureUsage.Sampled, TextureUsage.Sampled)]
        [Theory]
        public void Copy_WithOffsets_2D(TextureUsage srcUsage, TextureUsage dstUsage)
        {
            Texture src = RF.CreateTexture(TextureDescription.Texture2D(
                100, 100, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, srcUsage));

            Texture dst = RF.CreateTexture(TextureDescription.Texture2D(
                100, 100, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, dstUsage));

            RgbaByte[] srcData = new RgbaByte[src.Height * src.Width];
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                {
                    srcData[y * src.Width + x] = new RgbaByte((byte)x, (byte)y, 0, 1);
                }

            GD.UpdateTexture(src, srcData, 0, 0, 0, src.Width, src.Height, 1, 0, 0);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(
                src,
                50, 50, 0, 0, 0,
                dst,
                10, 10, 0, 0, 0,
                50, 50, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            Texture readback = GetReadback(dst);
            MappedResourceView<RgbaByte> readView = GD.Map<RgbaByte>(readback, MapMode.Read);
            for (int y = 10; y < 60; y++)
                for (int x = 10; x < 60; x++)
                {
                    Assert.Equal(new RgbaByte((byte)(x + 40), (byte)(y + 40), 0, 1), readView[x, y]);
                }
            GD.Unmap(readback);
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
        public unsafe void Update_NonMultipleOfFourWithCompressedTexture_2D()
        {
            Texture tex2D = RF.CreateTexture(TextureDescription.Texture2D(
                2, 2, 1, 1, PixelFormat.BC1_Rgb_UNorm, TextureUsage.Staging));

            byte[] data = new byte[16];

            fixed (byte* dataPtr = &data[0])
            {
                GD.UpdateTexture(
                    tex2D, (IntPtr)dataPtr, (uint)data.Length,
                    0, 0, 0,
                    4, 4, 1,
                    0, 0);
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

        [Theory]
        [MemberData(nameof(FormatCoverageData))]
        public unsafe void FormatCoverage_CopyThenRead(
            PixelFormat format, int rBits, int gBits, int bBits, int aBits,
            TextureType srcType,
            uint srcWidth, uint srcHeight, uint srcDepth, uint srcMipLevels, uint srcArrayLayers,
            TextureType dstType,
            uint dstWidth, uint dstHeight, uint dstDepth, uint dstMipLevels, uint dstArrayLayers,
            uint copyWidth, uint copyHeight, uint copyDepth,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel, uint srcArrayLayer,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel, uint dstArrayLayer)
        {
            if (!GD.GetPixelFormatSupport(format, srcType, TextureUsage.Staging))
            {
                return;
            }

            Texture srcTex = RF.CreateTexture(new TextureDescription(
                srcWidth, srcHeight, srcDepth, srcMipLevels, srcArrayLayers,
                format, TextureUsage.Staging, srcType));

            TextureDataReaderWriter tdrw = new TextureDataReaderWriter(rBits, gBits, bBits, aBits);
            byte[] dataArray = tdrw.GetDataArray(srcWidth, srcHeight, srcDepth);
            long rowPitch = srcWidth * tdrw.PixelBytes;
            long depthPitch = rowPitch * srcHeight;
            fixed (byte* dataPtr = dataArray)
            {
                for (uint z = 0; z < srcDepth; z++)
                {
                    for (uint y = 0; y < srcHeight; y++)
                    {
                        for (uint x = 0; x < srcWidth; x++)
                        {
                            long offset = z * depthPitch + y * rowPitch + x * tdrw.PixelBytes;
                            WidePixel pixel = tdrw.GetTestPixel(x, y, z);
                            tdrw.WritePixel(dataPtr + offset, pixel);
                        }
                    }
                }

                GD.UpdateTexture(
                    srcTex, (IntPtr)dataPtr, (uint)dataArray.Length,
                    0, 0, 0, srcWidth, srcHeight, srcDepth, 0, 0);
            }

            Texture dstTex = RF.CreateTexture(new TextureDescription(
                dstWidth, dstHeight, dstDepth, dstMipLevels, dstArrayLayers,
                format, TextureUsage.Staging, dstType));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();

            cl.CopyTexture(
                srcTex, srcX, srcY, srcZ, srcMipLevel, srcArrayLayer,
                dstTex, dstX, dstY, dstZ, dstMipLevel, dstArrayLayer,
                copyWidth, copyHeight, copyDepth, 1);

            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            MappedResource map = GD.Map(dstTex, MapMode.Read);
            for (uint z = 0; z < copyDepth; z++)
            {
                for (uint y = 0; y < copyHeight; y++)
                {
                    for (uint x = 0; x < copyWidth; x++)
                    {
                        long offset = (z + dstZ) * map.DepthPitch
                            + (y + dstY) * map.RowPitch
                            + (x + dstX) * tdrw.PixelBytes;
                        WidePixel expected = tdrw.GetTestPixel(x, y, z);
                        WidePixel actual = tdrw.ReadPixel((byte*)map.Data + offset);
                        Assert.Equal(expected, actual);
                    }
                }
            }

            GD.Unmap(dstTex);
        }

        public static IEnumerable<object[]> FormatCoverageData()
        {
            foreach (FormatProps props in s_allFormatProps)
            {
                yield return new object[]
                {
                    props.Format, props.RedBits, props.GreenBits, props.BlueBits, props.AlphaBits,
                    TextureType.Texture2D,
                    64, 64, 1, 1, 1,
                    TextureType.Texture2D,
                    64, 64, 1, 1, 1,
                    64, 64, 1,
                    0, 0, 0,
                    0, 0,
                    0, 0, 0,
                    0, 0
                };
            }
        }

        [Theory]
        [InlineData(TextureUsage.Sampled | TextureUsage.GenerateMipmaps)]
        [InlineData(TextureUsage.RenderTarget | TextureUsage.GenerateMipmaps)]
        [InlineData(TextureUsage.Storage | TextureUsage.GenerateMipmaps)]
        [InlineData(TextureUsage.Sampled | TextureUsage.RenderTarget | TextureUsage.GenerateMipmaps)]
        public unsafe void GenerateMipmaps(TextureUsage usage)
        {
            TextureDescription texDesc = TextureDescription.Texture2D(
                1024, 1024, 11, 1,
                PixelFormat.R32_G32_B32_A32_Float,
                usage);
            Texture tex = RF.CreateTexture(texDesc);

            texDesc.Usage = TextureUsage.Staging;
            Texture readback = RF.CreateTexture(texDesc);

            RgbaFloat[] pixelData = Enumerable.Repeat(RgbaFloat.Red, 1024 * 1024).ToArray();
            fixed (RgbaFloat* pixelDataPtr = pixelData)
            {
                GD.UpdateTexture(tex, (IntPtr)pixelDataPtr, 1024 * 1024 * 16, 0, 0, 0, 1024, 1024, 1, 0, 0);
            }

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.GenerateMipmaps(tex);
            cl.CopyTexture(tex, readback);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            for (uint level = 1; level < 11; level++)
            {
                MappedResourceView<RgbaFloat> readView = GD.Map<RgbaFloat>(readback, MapMode.Read, level);
                uint mipWidth = Math.Max(1, (uint)(tex.Width / Math.Pow(2, level)));
                uint mipHeight = Math.Max(1, (uint)(tex.Width / Math.Pow(2, level)));
                Assert.Equal(RgbaFloat.Red, readView[mipWidth - 1, mipHeight - 1]);
                GD.Unmap(readback, level);
            }
        }

        [Fact]
        public void CopyTexture_SmallCompressed()
        {
            Texture src = RF.CreateTexture(TextureDescription.Texture2D(16, 16, 4, 1, PixelFormat.BC3_UNorm, TextureUsage.Staging));
            Texture dst = RF.CreateTexture(TextureDescription.Texture2D(16, 16, 4, 1, PixelFormat.BC3_UNorm, TextureUsage.Sampled));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(
                src, 0, 0, 0, 3, 0,
                dst, 0, 0, 0, 3, 0,
                4, 4, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();
        }

        [Theory]
        [InlineData(PixelFormat.BC1_Rgb_UNorm)]
        [InlineData(PixelFormat.BC1_Rgb_UNorm_SRgb)]
        [InlineData(PixelFormat.BC1_Rgba_UNorm)]
        [InlineData(PixelFormat.BC1_Rgba_UNorm_SRgb)]
        [InlineData(PixelFormat.BC2_UNorm)]
        [InlineData(PixelFormat.BC2_UNorm_SRgb)]
        [InlineData(PixelFormat.BC3_UNorm)]
        [InlineData(PixelFormat.BC3_UNorm_SRgb)]
        [InlineData(PixelFormat.BC4_UNorm)]
        [InlineData(PixelFormat.BC4_SNorm)]
        [InlineData(PixelFormat.BC5_UNorm)]
        [InlineData(PixelFormat.BC5_SNorm)]
        [InlineData(PixelFormat.BC7_UNorm)]
        [InlineData(PixelFormat.BC7_UNorm_SRgb)]
        public void CreateSmallTexture(PixelFormat format)
        {
            Texture tex = RF.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, format, TextureUsage.Sampled));
            Assert.Equal(1u, tex.Width);
            Assert.Equal(1u, tex.Height);
        }

        private static readonly FormatProps[] s_allFormatProps =
        {
            new FormatProps(PixelFormat.R8_UNorm, 8, 0, 0, 0),
            new FormatProps(PixelFormat.R8_SNorm, 8, 0, 0, 0),
            new FormatProps(PixelFormat.R8_UInt, 8, 0, 0, 0),
            new FormatProps(PixelFormat.R8_SInt, 8, 0, 0, 0),

            new FormatProps(PixelFormat.R16_UNorm, 16, 0, 0, 0),
            new FormatProps(PixelFormat.R16_SNorm, 16, 0, 0, 0),
            new FormatProps(PixelFormat.R16_UInt, 16, 0, 0, 0),
            new FormatProps(PixelFormat.R16_SInt, 16, 0, 0, 0),
            new FormatProps(PixelFormat.R16_Float, 16, 0, 0, 0),

            new FormatProps(PixelFormat.R32_UInt, 32, 0, 0, 0),
            new FormatProps(PixelFormat.R32_SInt, 32, 0, 0, 0),
            new FormatProps(PixelFormat.R32_Float, 32, 0, 0, 0),

            new FormatProps(PixelFormat.R8_G8_UNorm, 8, 8, 0, 0),
            new FormatProps(PixelFormat.R8_G8_SNorm, 8, 8, 0, 0),
            new FormatProps(PixelFormat.R8_G8_UInt, 8, 8, 0, 0),
            new FormatProps(PixelFormat.R8_G8_SInt, 8, 8, 0, 0),

            new FormatProps(PixelFormat.R16_G16_UNorm, 16, 16, 0, 0),
            new FormatProps(PixelFormat.R16_G16_SNorm, 16, 16, 0, 0),
            new FormatProps(PixelFormat.R16_G16_UInt, 16, 16, 0, 0),
            new FormatProps(PixelFormat.R16_G16_SInt, 16, 16, 0, 0),
            new FormatProps(PixelFormat.R16_G16_Float, 16, 16, 0, 0),

            new FormatProps(PixelFormat.R32_G32_UInt, 32, 32, 0, 0),
            new FormatProps(PixelFormat.R32_G32_SInt, 32, 32, 0, 0),
            new FormatProps(PixelFormat.R32_G32_Float, 32, 32, 0, 0),

            new FormatProps(PixelFormat.B8_G8_R8_A8_UNorm, 8, 8, 8, 8),
            new FormatProps(PixelFormat.R8_G8_B8_A8_UNorm, 8, 8, 8, 8),
            new FormatProps(PixelFormat.R8_G8_B8_A8_SNorm, 8, 8, 8, 8),
            new FormatProps(PixelFormat.R8_G8_B8_A8_UInt, 8, 8, 8, 8),
            new FormatProps(PixelFormat.R8_G8_B8_A8_SInt, 8, 8, 8, 8),

            new FormatProps(PixelFormat.R16_G16_B16_A16_UNorm, 16, 16, 16, 16),
            new FormatProps(PixelFormat.R16_G16_B16_A16_SNorm, 16, 16, 16, 16),
            new FormatProps(PixelFormat.R16_G16_B16_A16_UInt, 16, 16, 16, 16),
            new FormatProps(PixelFormat.R16_G16_B16_A16_SInt, 16, 16, 16, 16),
            new FormatProps(PixelFormat.R16_G16_B16_A16_Float, 16, 16, 16, 16),

            new FormatProps(PixelFormat.R32_G32_B32_A32_UInt, 32, 32, 32, 32),
            new FormatProps(PixelFormat.R32_G32_B32_A32_SInt, 32, 32, 32, 32),
            new FormatProps(PixelFormat.R32_G32_B32_A32_Float, 32, 32, 32, 32),

            new FormatProps(PixelFormat.R10_G10_B10_A2_UInt, 10, 10, 10, 2),
            new FormatProps(PixelFormat.R10_G10_B10_A2_UNorm, 10, 10, 10, 2),
            new FormatProps(PixelFormat.R11_G11_B10_Float, 11, 11, 10, 0)
        };

        struct FormatProps
        {
            public readonly PixelFormat Format;
            public readonly int RedBits;
            public readonly int BlueBits;
            public readonly int GreenBits;
            public readonly int AlphaBits;

            public FormatProps(PixelFormat format, int redBits, int blueBits, int greenBits, int alphaBits)
            {
                Format = format;
                RedBits = redBits;
                BlueBits = blueBits;
                GreenBits = greenBits;
                AlphaBits = alphaBits;
            }
        }
    }

#if TEST_VULKAN
    [Trait("Backend", "Vulkan")]
    public class VulkanTextureTests : TextureTestBase<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    [Trait("Backend", "D3D11")]
    public class D3D11TextureTests : TextureTestBase<D3D11DeviceCreator> { }
#endif
#if TEST_METAL
    [Trait("Backend", "Metal")]
    public class MetalTextureTests : TextureTestBase<MetalDeviceCreator> { }
#endif
#if TEST_OPENGL
    [Trait("Backend", "OpenGL")]
    public class OpenGLTextureTests : TextureTestBase<OpenGLDeviceCreator> { }
#endif
#if TEST_OPENGLES
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESTextureTests : TextureTestBase<OpenGLESDeviceCreator> { }
#endif
}
