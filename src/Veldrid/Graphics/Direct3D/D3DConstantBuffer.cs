using System;
using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DConstantBuffer : D3DBuffer, ConstantBuffer
    {
        public D3DConstantBuffer(Device device, int sizeInBytes)
            : base(device, sizeInBytes, BindFlags.ConstantBuffer, ResourceUsage.Default)
        { }
    }
}