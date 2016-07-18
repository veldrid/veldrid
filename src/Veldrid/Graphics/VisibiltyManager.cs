using System.Numerics;

namespace Veldrid.Graphics
{
    public interface VisibiltyManager
    {
        void CollectVisibleObjects(RenderQueue queue, string pipelineStage, Vector3 viewPosition);
        void CollectVisibleObjects(RenderQueue queue, string pipelineStage, ref BoundingFrustum visibleFrustum, Vector3 viewPosition);
    }
}
