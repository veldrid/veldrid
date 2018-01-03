using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLBuffer : DeviceBuffer
    {
        private string _name;
        private bool _disposed;

        public override uint SizeInBytes { get; }
        public override BufferUsage Usage { get; }

        public override string Name
        {
            get => _name;
            set
            {
                NSString nameNSS = NSString.New(value);
                DeviceBuffer.addDebugMarker(nameNSS, new NSRange(0, SizeInBytes));
                ObjectiveCRuntime.release(nameNSS.NativePtr);
                _name = value;
            }
        }

        public MetalBindings.MTLBuffer DeviceBuffer { get; private set; }

        public MTLBuffer(ref BufferDescription bd, MTLGraphicsDevice gd)
        {
            SizeInBytes = bd.SizeInBytes;
            Usage = bd.Usage;
            DeviceBuffer = gd.Device.newBufferWithLengthOptions((UIntPtr)SizeInBytes, 0);
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                ObjectiveCRuntime.release(DeviceBuffer.NativePtr);
            }
        }
    }
}