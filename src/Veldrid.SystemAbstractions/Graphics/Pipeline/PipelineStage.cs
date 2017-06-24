using System.Numerics;

namespace Veldrid.Graphics.Pipeline
{
    public interface PipelineStage
    {
        string Name { get; }
        bool Enabled { get; set; }
        RenderContext RenderContext { get; }
        void ExecuteStage(VisibiltyManager visibilityManager, Vector3 viewPosition);
        void ChangeRenderContext(RenderContext rc);
    }
}
