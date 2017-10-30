using System;

namespace Veldrid
{
    public abstract class Pipeline : DeviceResource, IDisposable
    {
        public abstract void Dispose();
    }
}
