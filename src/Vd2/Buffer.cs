using System;

namespace Vd2
{
    public interface Buffer : IDisposable
    {
        ulong SizeInBytes { get; }
    }
}
