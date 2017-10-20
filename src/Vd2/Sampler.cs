using System;

namespace Vd2
{
    public abstract class Sampler : BindableResource, IDisposable
    {
        public abstract void Dispose();
    }
}
