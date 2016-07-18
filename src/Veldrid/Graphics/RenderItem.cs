using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Graphics
{
    public interface RenderItem
    {
        IEnumerable<string> GetStagesParticipated();
        void Render(RenderContext rc, string pipelineStage);
        RenderOrderKey GetRenderOrderKey(Vector3 viewPosition);
        bool Cull(ref BoundingFrustum visibleFrustum);
    }
}