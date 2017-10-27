using System;

namespace Veldrid
{
    public abstract class Sampler : BindableResource, IDisposable
    {
        public abstract void Dispose();
    }
}
