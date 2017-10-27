using SharpDX.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11VertexBuffer : D3D11Buffer, VertexBuffer
    {
        public D3D11VertexBuffer(Device device, ref BufferDescription description)
            : base(device, description.SizeInBytes, BindFlags.VertexBuffer)
        {
        }
    }
}
