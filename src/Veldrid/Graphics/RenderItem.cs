using System.Collections.Generic;

namespace Veldrid.Graphics
{
    public interface RenderItem
    {
        IEnumerable<string> GetStagesParticipated();
        void Render(RenderContext rc, string pipelineStage);
        RenderOrderKey GetRenderOrderKey();
    }
}