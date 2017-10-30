using System;

namespace Veldrid
{
    public abstract class Shader : DeviceResource, IDisposable
    {
        public abstract void Dispose();
    }
}
