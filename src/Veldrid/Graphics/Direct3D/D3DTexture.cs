using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DTexture : IDisposable
    {
        public Texture2D DeviceTexture { get; }

        public D3DTexture(Device device, Texture texture)
        {
            Texture2DDescription desc;
            desc.Width = texture.Width;
            desc.Height = texture.Height;
            desc.ArraySize = 1;
            desc.BindFlags = BindFlags.ShaderResource;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.Format = SharpDX.DXGI.Format.R32G32B32A32_Float;
            desc.MipLevels = 1;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;

            unsafe
            {
                fixed (float* pixelPtr = texture.Pixels)
                {
                    int stride = texture.Width * texture.PixelSizeInBytes;
                    DataRectangle dataRectangle = new DataRectangle(new System.IntPtr(pixelPtr), stride);
                    DeviceTexture = new Texture2D(device, desc, dataRectangle);

                }
            }
        }

        public void Dispose()
        {
            DeviceTexture.Dispose();
        }
    }
}
