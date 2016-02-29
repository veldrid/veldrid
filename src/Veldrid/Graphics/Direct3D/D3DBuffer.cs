using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DBuffer : System.IDisposable
    {
        public Buffer Buffer { get; }
        protected Device Device { get; }

        public D3DBuffer(Device device, BindFlags bindFlags, ResourceUsage resourceUsage)
        {
            Device = device;
            Buffer = new Buffer(device, new BufferDescription(1, bindFlags, resourceUsage));
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }

        protected void UpdateBufferData<T>(T[] data) where T : struct
        {
            Device.ImmediateContext.UpdateSubresource(data, Buffer);
        }

        protected void UpdateBufferData<T>(ref T data) where T : struct
        {
            Device.ImmediateContext.UpdateSubresource(ref data, Buffer);
        }
    }
}
