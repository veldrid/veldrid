using SharpDX.Direct3D11;

namespace Vd2.D3D11
{
    internal class D3D11Buffer : Buffer
    {
        private readonly Device _device;
        private readonly SharpDX.Direct3D11.Buffer _buffer;

        public ulong SizeInBytes { get; }

        public SharpDX.Direct3D11.Buffer Buffer => _buffer;

        public D3D11Buffer(Device device, ulong sizeInBytes, BindFlags bindFlags)
        {
            _device = device;
            SharpDX.Direct3D11.BufferDescription bd = new SharpDX.Direct3D11.BufferDescription((int)sizeInBytes, bindFlags, ResourceUsage.Default);
            _buffer = new SharpDX.Direct3D11.Buffer(_device, bd);
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }
    }
}
