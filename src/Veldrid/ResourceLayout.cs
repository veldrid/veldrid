using System;

namespace Veldrid
{
    public abstract class ResourceLayout : DeviceResource, IDisposable
    {
        public abstract void Dispose();
    }
}
