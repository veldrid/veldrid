using System.Numerics;

namespace Veldrid.Graphics
{
    public interface VisibiltyManager
    {
        void CollectVisibleObjects(RenderQueue queue, string pipelineStage, Vector3 position, Vector3 direction); // TODO: Implement visibility culling with real arguments.
    }
}
