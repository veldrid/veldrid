using SharpDX.Direct3D11;
using System;

namespace Vd2.D3D11
{
    internal class D3D11TextureView : TextureView
    {
        public ShaderResourceView ShaderResourceView { get; }

        public D3D11TextureView(Device device, ref TextureViewDescription description)
        {
            if (description.Target is D3D11Texture2D d3dTex2d)
            {
                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription
                {
                    Format = d3dTex2d.DeviceTexture.Description.Format,
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource
                    {
                        MipLevels = (int)d3dTex2d.MipLevels,
                        MostDetailedMip = 0
                    }
                };

                ShaderResourceView = new ShaderResourceView(device, d3dTex2d.DeviceTexture, srvDesc);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}