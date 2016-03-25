using System;

namespace Veldrid.Graphics
{
    public interface DepthStencilState : RenderStateModifier, IDisposable
    {
        bool IsDepthEnabled { get; }
        DepthComparison DepthComparison { get; }
    }
}
