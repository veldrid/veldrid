using System;

namespace Veldrid
{
    public interface Buffer : DeviceResource, IDisposable
    {
        ulong SizeInBytes { get; }
    }
}
