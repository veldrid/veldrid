using SharpDX.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11Buffer : Buffer
    {
        private readonly SharpDX.Direct3D11.Buffer _buffer;

        public override ulong SizeInBytes { get; }

        public override BufferUsage Usage { get; }

        public SharpDX.Direct3D11.Buffer Buffer => _buffer;

        public D3D11Buffer(Device device, ulong sizeInBytes, BufferUsage usage)
        {
            SizeInBytes = sizeInBytes;
            Usage = usage;
            SharpDX.Direct3D11.BufferDescription bd = new SharpDX.Direct3D11.BufferDescription(
                (int)sizeInBytes,
                D3D11Formats.VdToD3D11BindFlags(usage),
                ResourceUsage.Default);
            _buffer = new SharpDX.Direct3D11.Buffer(device, bd);
        }

        public override void Dispose()
        {
            _buffer.Dispose();
        }
    }
}
