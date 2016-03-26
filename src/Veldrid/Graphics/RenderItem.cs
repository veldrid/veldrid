using System.Collections;
using System.Collections.Generic;

namespace Veldrid.Graphics
{
    public interface RenderItem
    {
        IEnumerable<string> GetStagesParticipated();
        void Render(RenderContext context, string pipelineStage);
        RenderOrderKey GetRenderOrderKey();
        void ChangeRenderContext(RenderContext context);
    }
}