using SharpDX.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11IndexBuffer : D3D11Buffer, IndexBuffer
    {
        public IndexFormat Format { get; }

        public D3D11IndexBuffer(Device device, ref IndexBufferDescription description)
            : base(device, description.SizeInBytes, BindFlags.IndexBuffer)
        {
            Format = description.Format;
        }
    }
}