using System;
using SharpDX.Direct3D11;

namespace Vd2.D3D11
{
    internal class D3D11TextureCube : TextureCube
    {
        private readonly Device _device;

        public override PixelFormat Format { get; }

        public override uint MipLevels { get; }

        public override uint ArrayLayers { get; }

        public override TextureUsage Usage { get; }

        public D3D11TextureCube(Device device, ref TextureDescription description)
        {
            _device = device;
        }
    }
}