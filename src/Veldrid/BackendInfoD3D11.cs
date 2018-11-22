using System;
using Veldrid.D3D11;

namespace Veldrid
{
    public class BackendInfoD3D11
    {
        private readonly D3D11GraphicsDevice _gd;

        internal BackendInfoD3D11(D3D11GraphicsDevice gd)
        {
            _gd = gd;
        }

        public IntPtr Device => _gd.Device.NativePointer;

        public IntPtr GetTexturePointer(Texture texture)
            => Util.AssertSubtype<Texture, D3D11Texture>(texture).DeviceTexture.NativePointer;
    }
}
