namespace Veldrid.Graphics
{
    public interface DepthStencilState : RenderStateModifier
    {
        bool IsDepthEnabled { get; }
        DepthComparison DepthComparison { get; }
    }
}
