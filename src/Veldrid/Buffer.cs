using System;

namespace Veldrid
{
    public interface Buffer : IDisposable
    {
        ulong SizeInBytes { get; }
    }
}
