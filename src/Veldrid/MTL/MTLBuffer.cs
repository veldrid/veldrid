using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal sealed class MTLBuffer : DeviceBuffer
    {
        private string? _name;
        private bool _disposed;

        public override string? Name
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

        public unsafe MTLBuffer(in BufferDescription bd, MTLGraphicsDevice gd) : base(bd)
        {
            if (bd.InitialData == IntPtr.Zero)
            {
                DeviceBuffer = gd.Device.newBufferWithLengthOptions(
                    SizeInBytes,
                    0);
            }
            else
            {
                DeviceBuffer = gd.Device.newBuffer(
                    (void*)bd.InitialData,
                    SizeInBytes,
                    0);
            }
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
