namespace Veldrid.Graphics
{
    public interface RasterizerState : RenderStateModifier
    {
        FaceCullingMode CullMode { get; }
        TriangleFillMode FillMode { get; }
        bool IsDepthClipEnabled { get; }
        bool IsScissorTestEnabled { get; }
    }
}
