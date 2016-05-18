using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DTextureBinding : ShaderTextureBinding
    {
        public ShaderResourceView ResourceView { get; }
        public D3DTexture Texture { get; }
        DeviceTexture ShaderTextureBinding.BoundTexture => Texture;

        public D3DTextureBinding(ShaderResourceView srv, D3DTexture texture)
        {
            ResourceView = srv;
            Texture = texture;
        }

        public void Dispose()
        {
            ResourceView.Dispose();
        }
    }
}
