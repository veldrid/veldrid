using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DTexture : DeviceTexture, IDisposable, PixelDataProvider
    {
        private readonly Device _device;

        public Texture2D DeviceTexture { get; }

        public D3DTexture(Device device, Texture2DDescription description)
        {
            _device = device;
            DeviceTexture = new Texture2D(device, description);
        }

        public D3DTexture(Device device, Texture2D existingTexture)
        {
            _device = device;
            DeviceTexture = existingTexture;
        }

        public D3DTexture(
            Device device,
            BindFlags bindFlags,
            ResourceUsage usage,
            CpuAccessFlags cpuAccessFlags,
            SharpDX.DXGI.Format format,
            IntPtr pixelPtr,
            int width,
            int height,
            int stride)
        {
            _device = device;

            Texture2DDescription desc = CreateDescription(width, height, bindFlags, usage, cpuAccessFlags, format);

            unsafe
            {
                DataRectangle dataRectangle = new DataRectangle(pixelPtr, stride);
                DeviceTexture = new Texture2D(device, desc, dataRectangle);
            }
        }

        private Texture2DDescription CreateDescription(
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
            desc.MipLevels = 1;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;

            return desc;
        }

        public void CopyTo(TextureData textureData)
        {
            textureData.AcceptPixelData(this);
        }

        public void Dispose()
        {
            DeviceTexture.Dispose();
        }

        public unsafe void SetPixelData<T>(T[] destination, int width, int height, int pixelSizeInBytes) where T : struct
        {
            var destHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            var destPtr = destHandle.AddrOfPinnedObject();
            SetPixelData(destPtr, width, height, pixelSizeInBytes);
            destHandle.Free();
        }

        public unsafe void SetPixelData(IntPtr destPtr, int width, int height, int pixelSizeInBytes)
        {
            D3DTexture stagingTexture = new D3DTexture(_device, new Texture2DDescription()
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

            _device.ImmediateContext.CopyResource(DeviceTexture, stagingTexture.DeviceTexture);
            DataStream ds;
            var box = _device.ImmediateContext.MapSubresource(stagingTexture.DeviceTexture, 0, MapMode.Read, MapFlags.None, out ds);
            IntPtr pixelPtr = ds.DataPointer;
            Utilities.CopyMemory(destPtr, pixelPtr, width * height * pixelSizeInBytes);
            _device.ImmediateContext.UnmapSubresource(stagingTexture.DeviceTexture, 0);

            stagingTexture.Dispose();
        }
    }
}
