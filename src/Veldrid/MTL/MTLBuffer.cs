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

        public uint ActualCapacity { get; }

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

        public override bool IsDisposed => _disposed;

        public MetalBindings.MTLBuffer DeviceBuffer { get; private set; }

        public MTLBuffer(ref BufferDescription bd, MTLGraphicsDevice gd)
        {
            SizeInBytes = bd.SizeInBytes;
            uint roundFactor = (4 - (SizeInBytes % 4)) % 4;
            ActualCapacity = SizeInBytes + roundFactor;
            Usage = bd.Usage;

            var sharedMemory = Usage == BufferUsage.Staging || (Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic;
            var bufferOptions = sharedMemory ? MTLResourceOptions.StorageModeShared : MTLResourceOptions.StorageModePrivate;

            DeviceBuffer = gd.Device.newBufferWithLengthOptions(
                (UIntPtr)ActualCapacity,
                bufferOptions);
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
