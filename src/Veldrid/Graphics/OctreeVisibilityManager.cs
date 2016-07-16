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
        private OctreeNode<RenderItem> _octree = new OctreeNode<RenderItem>(new BoundingBox(Vector3.One * -50, Vector3.One * 50), 2);

        public OctreeNode<RenderItem> Octree => _octree;

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

        public void CollectVisibleObjects(RenderQueue queue, string pipelineStage)
        {
            _results.Clear();
            Func<RenderItem, bool> filter = GetFilter(pipelineStage);
            _octree.GetAllContainedObjects(_results, filter);
            queue.AddRange(_results);
            foreach (var item in _freeRenderItems)
            {
                if (filter(item))
                {
                    queue.Add(item);
                }
            }
        }

        public void CollectVisibleObjects(RenderQueue queue, string pipelineStage, ref BoundingFrustum visibleFrustum)
        {
            _results.Clear();
            Func<RenderItem, bool> filter = GetFilter(pipelineStage);
            _octree.GetContainedObjects(ref visibleFrustum, _results, filter);
            queue.AddRange(_results);
            foreach (var item in _freeRenderItems)
            {
                if (filter(item))
                {
                    queue.Add(item);
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
            _octree = _octree.AddItem(ref bounds, ri);
        }

        public void AddRenderItem(ref BoundingBox bounds, RenderItem ri)
        {
            _octree = _octree.AddItem(ref bounds, ri);
        }
    }
}
