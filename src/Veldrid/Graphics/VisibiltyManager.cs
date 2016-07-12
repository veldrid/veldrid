using System.Numerics;

namespace Veldrid.Graphics
{
    public interface VisibiltyManager
    {
        void CollectVisibleObjects(RenderQueue queue, string pipelineStage);
        void CollectVisibleObjects(RenderQueue queue, string pipelineStage, ref BoundingFrustum visibleFrustum);
    }
}
