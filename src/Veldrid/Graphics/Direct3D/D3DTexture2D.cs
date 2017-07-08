using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.Direct3D
{
    public abstract class D3DTexture : DeviceTexture, IDisposable
    {
        public abstract Texture2D DeviceTexture { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public int MipLevels { get; protected set; } = 1;

        public abstract ShaderResourceViewDescription GetShaderResourceViewDescription();

        public void Dispose()
        {
            DeviceTexture.Dispose();
        }
    }

    public class D3DTexture2D : D3DTexture, DeviceTexture2D, IDisposable
    {
        private readonly Device _device;

        public override Texture2D DeviceTexture { get; }

        public override int Width => DeviceTexture.Description.Width;

        public override int Height => DeviceTexture.Description.Height;

        public D3DTexture2D(Device device, Texture2DDescription description)
        {
            _device = device;
            DeviceTexture = new Texture2D(device, description);
        }

        public D3DTexture2D(Device device, Texture2D existingTexture)
        {
            _device = device;
            DeviceTexture = existingTexture;
        }

        public D3DTexture2D(
            Device device,
            BindFlags bindFlags,
            ResourceUsage usage,
            CpuAccessFlags cpuAccessFlags,
            SharpDX.DXGI.Format format,
            int mipLevels,
            int width,
            int height,
            int stride)
        {
            _device = device;
            MipLevels = mipLevels;
            Texture2DDescription desc = CreateDescription(mipLevels, width, height, bindFlags, usage, cpuAccessFlags, format);
            DeviceTexture = new Texture2D(device, desc);
        }

        private Texture2DDescription CreateDescription(
            int mipLevels,
            int width,
            int height,
            BindFlags bindFlags,
            ResourceUsage usage,
            CpuAccessFlags cpuAccessFlags,
            SharpDX.DXGI.Format format)
        {
            Texture2DDescription desc;
            desc.Width = width;
            desc.Height = height;
            desc.ArraySize = 1;
            desc.BindFlags = bindFlags;
            desc.Usage = usage;
            desc.CpuAccessFlags = cpuAccessFlags;
            desc.Format = format;
            desc.MipLevels = mipLevels;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;

            return desc;
        }

        public void SetTextureData(int mipLevel, int x, int y, int width, int height, IntPtr data, int dataSizeInBytes)
        {
            ResourceRegion resourceRegion = new ResourceRegion(
                left: x,
                top: y,
                front: 0,
                right: x + width,
                bottom: y + height,
                back: 1);
            int srcRowPitch = GetRowPitch(width);
            _device.ImmediateContext.UpdateSubresource(DeviceTexture, mipLevel, resourceRegion, data, srcRowPitch, 0);
        }

        public unsafe void SetPixelData<T>(T[] destination, int width, int height, int pixelSizeInBytes) where T : struct
        {
            var destHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            SetPixelData(destHandle.AddrOfPinnedObject(), width, height, pixelSizeInBytes);
            destHandle.Free();
        }

        public unsafe void SetPixelData(IntPtr destPtr, int width, int height, int pixelSizeInBytes)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            D3DTexture2D stagingTexture = new D3DTexture2D(_device, new Texture2DDescription()
            {
                Width = width,
                Height = height,
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Format = DeviceTexture.Description.Format
            });

            // Copy the data from the GPU to the staging texture.
            _device.ImmediateContext.CopySubresourceRegion(DeviceTexture, 0, null, stagingTexture.DeviceTexture, 0);

            int elementCount = width * height;
            // Copy the data to the array.
            DataStream ds = null;
            var db = _device.ImmediateContext.MapSubresource(
                stagingTexture.DeviceTexture,
                0,
                MapMode.Read,
                MapFlags.None,
                out ds);

            int rowSize = pixelSizeInBytes * width;
            // If the pitch exactly matches the row size, we can simply copy all the data.
            if (rowSize == db.RowPitch)
            {
                SharpDX.Utilities.CopyMemory(destPtr, db.DataPointer, elementCount * pixelSizeInBytes);
            }
            else
            {
                // The texture data may not have a pitch exactly equal to the row width.
                // This means that the pixel data is not "tightly packed" into the buffer given
                // to us, and has empty data at the end of each row.

                for (int rowNumber = 0; rowNumber < height; rowNumber++)
                {
                    int rowStartOffsetInBytes = rowNumber * width * pixelSizeInBytes;
                    ds.Read(destPtr, rowStartOffsetInBytes, width * pixelSizeInBytes);

                    // At the end of the row, seek the stream to skip the extra filler data,
                    // which is equal to (RowPitch - RowSize) bytes.
                    ds.Seek(db.RowPitch - rowSize, SeekOrigin.Current);
                }
            }

            stagingTexture.Dispose();
        }

        public override ShaderResourceViewDescription GetShaderResourceViewDescription()
        {
            ShaderResourceViewDescription srvd = new ShaderResourceViewDescription();
            srvd.Format = D3DFormats.MapFormatForShaderResourceView(DeviceTexture.Description.Format);
            srvd.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
            srvd.Texture2D.MipLevels = DeviceTexture.Description.MipLevels;
            srvd.Texture2D.MostDetailedMip = 0;

            return srvd;
        }

        private int GetRowPitch(int width)
        {
            var pixelSize = D3DFormats.GetPixelSize(DeviceTexture.Description.Format);
            return pixelSize * width;
        }

        public void GetTextureData<T>(int mipLevel, T[] destination) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            GetTextureData(mipLevel, handle.AddrOfPinnedObject(), Unsafe.SizeOf<T>() * destination.Length);
            handle.Free();
        }

        public void GetTextureData(int mipLevel, IntPtr destination, int storageSizeInBytes)
        {
            int width = MipmapHelper.GetDimension(Width, mipLevel);
            int height = MipmapHelper.GetDimension(Height, mipLevel);

            D3DTexture2D stagingTexture = new D3DTexture2D(_device, new Texture2DDescription()
            {
                Width = width,
                Height = height,
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Format = DeviceTexture.Description.Format
            });

            // Copy the data from the GPU to the staging texture.
            _device.ImmediateContext.CopySubresourceRegion(DeviceTexture, mipLevel, null, stagingTexture.DeviceTexture, 0);

            int elementCount = width * height;
            // Copy the data to the array.
            DataBox db = _device.ImmediateContext.MapSubresource(
                stagingTexture.DeviceTexture,
                0,
                MapMode.Read,
                MapFlags.None,
                out DataStream ds);

            int pixelSizeInBytes = D3DFormats.GetPixelSize(DeviceTexture.Description.Format);
            int rowSize = pixelSizeInBytes * width;
            // If the pitch exactly matches the row size, we can simply copy all the data.
            if (rowSize == db.RowPitch)
            {
                SharpDX.Utilities.CopyMemory(destination, db.DataPointer, elementCount * pixelSizeInBytes);
            }
            else
            {
                // The texture data may not have a pitch exactly equal to the row width.
                // This means that the pixel data is not "tightly packed" into the buffer given
                // to us, and has empty data at the end of each row.

                for (int rowNumber = 0; rowNumber < height; rowNumber++)
                {
                    int rowStartOffsetInBytes = rowNumber * width * pixelSizeInBytes;
                    ds.Read(destination, rowStartOffsetInBytes, width * pixelSizeInBytes);

                    // At the end of the row, seek the stream to skip the extra filler data,
                    // which is equal to (RowPitch - RowSize) bytes.
                    ds.Seek(db.RowPitch - rowSize, SeekOrigin.Current);
                }
            }

            stagingTexture.Dispose();
        }
    }
}