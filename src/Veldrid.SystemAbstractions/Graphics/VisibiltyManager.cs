using System.Numerics;

namespace Veldrid.Graphics
{
    /// <summary>
    /// An interface which encapsulates generic visibility querying.
    /// </summary>
    public interface VisibiltyManager
    {
        /// <summary>
        /// Collects objects which are visible from the given position into the <see cref="RenderQueue"/>,
        /// and which belong to the given pipeline stage.
        /// </summary>
        /// <param name="queue">The queue to store collected objects. The queue is not cleared before objects are added.</param>
        /// <param name="pipelineStage">The name of the pipeline stage which collected objects belong to.</param>
        /// <param name="viewPosition">The view position.</param>
        void CollectVisibleObjects(RenderQueue queue, string pipelineStage, Vector3 viewPosition);

        /// <summary>
        /// Collects objects which are visible from the given position into the <see cref="RenderQueue"/>,
        /// and which belong to the given pipeline stage.
        /// </summary>
        /// <param name="queue">The queue to store collected objects. The queue is not cleared before objects are added.</param>
        /// <param name="pipelineStage">The name of the pipeline stage which collected objects belong to.</param>
        /// <param name="visibleFrustum">A frustum describing the visible portion of the world.
        /// Objects outside this frustum are not collected.</param>
        /// <param name="viewPosition">The view position.</param>
        void CollectVisibleObjects(RenderQueue queue, string pipelineStage, ref BoundingFrustum visibleFrustum, Vector3 viewPosition);
    }
}
