using System;

namespace Veldrid.Graphics
{
    public interface DepthStencilState : IDisposable
    {
        bool IsDepthEnabled { get; }
        DepthComparison DepthComparison { get; }
    }
}
