using SharpDX.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11UniformBuffer : D3D11Buffer, UniformBuffer
    {
        public D3D11UniformBuffer(Device device, ref BufferDescription description)
            : base(device, description.SizeInBytes, BindFlags.ConstantBuffer)
        {
        }
    }
}