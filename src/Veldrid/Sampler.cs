using System;

namespace Veldrid
{
    public abstract class Sampler : DeviceResource, BindableResource, IDisposable
    {
        public abstract void Dispose();
    }
}
