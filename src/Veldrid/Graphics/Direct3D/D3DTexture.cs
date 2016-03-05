using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DTexture : DeviceTexture, IDisposable
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
            int width, int height,
            BindFlags bindFlags,
            ResourceUsage usage,
            CpuAccessFlags cpuAccessFlags,
            SharpDX.DXGI.Format format)
        {
            _device = device;
            Texture2DDescription desc = CreateDescription(width, height, bindFlags, usage, cpuAccessFlags, format);
            DeviceTexture = new Texture2D(device, desc);
        }

        public D3DTexture(
            Device device,
            BindFlags bindFlags,
            ResourceUsage usage,
            CpuAccessFlags cpuAccessFlags,
            SharpDX.DXGI.Format format,
            Texture texture)
        {
            _device = device;

            Texture2DDescription desc = CreateDescription(texture.Width, texture.Height, bindFlags, usage, cpuAccessFlags, format);

            unsafe
            {
                fixed (float* pixelPtr = texture.Pixels)
                {
                    int stride = texture.Width * texture.PixelSizeInBytes;
                    DataRectangle dataRectangle = new DataRectangle(new IntPtr(pixelPtr), stride);
                    DeviceTexture = new Texture2D(device, desc, dataRectangle);
                }
            }

            Texture cpuTextureTest = new ImageProcessorTexture(new ImageProcessor.Image(texture.Width, texture.Height));
            CopyTo(cpuTextureTest);
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

        public unsafe void CopyTo(Texture texture)
        {
            D3DTexture stagingTexture = new D3DTexture(
                _device,
                texture.Width,
                texture.Height,
                BindFlags.None,
                ResourceUsage.Staging,
                CpuAccessFlags.Read,
                SharpDX.DXGI.Format.R32G32B32A32_Float);

            _device.ImmediateContext.CopyResource(DeviceTexture, stagingTexture.DeviceTexture);
            var box = _device.ImmediateContext.MapSubresource(stagingTexture.DeviceTexture, 0, MapMode.Read, MapFlags.None);
            float* pixelPtr = (float*)box.DataPointer.ToPointer();
            for (int i = 0; i < texture.Pixels.Length; i++)
            {
                texture.Pixels[i] = pixelPtr[i];
            }

            _device.ImmediateContext.UnmapSubresource(stagingTexture.DeviceTexture, 0);
            stagingTexture.Dispose();
        }

        public void Dispose()
        {
            DeviceTexture.Dispose();
        }
    }
}
