using System;

namespace Veldrid.Graphics
{
    public interface RasterizerState : RenderStateModifier, IDisposable
    {
        FaceCullingMode CullMode { get; }
        TriangleFillMode FillMode { get; }
        bool IsDepthClipEnabled { get; }
        bool IsScissorTestEnabled { get; }
    }
}
