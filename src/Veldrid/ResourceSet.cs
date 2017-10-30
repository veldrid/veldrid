using System;

namespace Veldrid
{
    public abstract class ResourceSet : DeviceResource, IDisposable
    {
        public abstract void Dispose();
    }
}
