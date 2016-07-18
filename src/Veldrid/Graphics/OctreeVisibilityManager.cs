using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Veldrid.Graphics
{
    public class OctreeVisibilityManager : VisibiltyManager
    {
        private readonly List<RenderItem> _results = new List<RenderItem>();
        private readonly List<RenderItem> _freeRenderItems = new List<RenderItem>();
        private readonly Dictionary<string, Func<RenderItem, bool>> _filters = new Dictionary<string, Func<RenderItem, bool>>();
        private Octree<RenderItem> _octree = new Octree<RenderItem>(new BoundingBox(Vector3.One * -50, Vector3.One * 50), 2);

        public OctreeNode<RenderItem> OctreeRootNode => _octree.CurrentRoot;

        public IEnumerable<RenderItem> RenderItems
        {
            get
            {
                foreach (var item in _freeRenderItems)
                {
                    yield return item;
                }

                _results.Clear();
                _octree.GetAllContainedObjects(_results, null);
                foreach (var item in _results)
                {
                    yield return item;
                }
            }
        }

        public void CollectVisibleObjects(RenderQueue queue, string pipelineStage, Vector3 viewPosition)
        {
            _results.Clear();
            Func<RenderItem, bool> filter = GetFilter(pipelineStage);
            _octree.GetAllContainedObjects(_results, filter);
            queue.AddRange(_results, viewPosition);
            foreach (var item in _freeRenderItems)
            {
                if (filter(item))
                {
                    queue.Add(item, viewPosition);
                }
            }
        }

        public void CollectVisibleObjects(RenderQueue queue, string pipelineStage, ref BoundingFrustum visibleFrustum, Vector3 viewPosition)
        {
            _results.Clear();
            Func<RenderItem, bool> filter = GetFilter(pipelineStage);
            _octree.GetContainedObjects(visibleFrustum, _results, filter);
            queue.AddRange(_results, viewPosition);
            foreach (var item in _freeRenderItems)
            {
                if (filter(item))
                {
                    queue.Add(item, viewPosition);
                }
            }
        }

        private Func<RenderItem, bool> GetFilter(string pipelineStage)
        {
            Func<RenderItem, bool> filter;
            if (!_filters.TryGetValue(pipelineStage, out filter))
            {
                filter = (ri) => ri.GetStagesParticipated().Contains(pipelineStage);
                _filters.Add(pipelineStage, filter);
            }

            return filter;
        }

        public void AddRenderItem(RenderItem ri)
        {
            _freeRenderItems.Add(ri);
        }

        public void AddRenderItem(BoundingBox bounds, RenderItem ri)
        {
            _octree.AddItem(bounds, ri);
        }

        public void AddRenderItem(ref BoundingBox bounds, RenderItem ri)
        {
            _octree.AddItem(bounds, ri);
        }
    }
}
