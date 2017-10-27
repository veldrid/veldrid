using System;

namespace Veldrid
{
    public abstract class Pipeline : IDisposable
    {
        public abstract void Dispose();
    }
}
