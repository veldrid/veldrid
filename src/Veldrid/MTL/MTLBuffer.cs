using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLBuffer : DeviceBuffer
    {
        private string _name;
        private bool _disposed;
        private MetalBindings.MTLBuffer _deviceBuffer;

        public override uint SizeInBytes { get; }
        public override BufferUsage Usage { get; }

        public uint ActualCapacity { get; }

        public override string Name
        {
            get => _name;
            set
            {
                NSString nameNSS = NSString.New(value);
                DeviceBuffer.addDebugMarker(nameNSS, new NSRange(0, SizeInBytes));
                ObjectiveCRuntime.release(nameNSS.NativePtr);

                _deviceBuffer.label = value;

                _name = value;
            }
        }

        public MetalBindings.MTLBuffer DeviceBuffer => _deviceBuffer;

        public MTLBuffer(ref BufferDescription bd, MTLGraphicsDevice gd)
            : base(gd)
        {
            SizeInBytes = bd.SizeInBytes;
            uint roundFactor = (4 - (SizeInBytes % 4)) % 4;
            ActualCapacity = SizeInBytes + roundFactor;
            Usage = bd.Usage;
            _deviceBuffer = gd.Device.newBufferWithLengthOptions(
                (UIntPtr)ActualCapacity,
                0);
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                ObjectiveCRuntime.release(_deviceBuffer.NativePtr);
            }
        }
    }
}
