using System;

namespace Vd2
{
    public interface Buffer
    {
        ulong SizeInBytes { get; }
        void Dispose();
    }
}
