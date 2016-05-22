using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A visiblity list which stores objects in a flat, nonhierarchical list.
    /// </summary>
    public class FlatListVisibilityManager : VisibiltyManager
    {
        private readonly Dictionary<string, List<RenderItem>> _renderItemsByStage = new Dictionary<string, List<RenderItem>>();
        private readonly HashSet<RenderItem> _distinctRenderItems = new HashSet<RenderItem>();

        public IReadOnlyCollection<RenderItem> RenderItems => _distinctRenderItems;

        public void AddRenderItem(RenderItem item)
        {
            foreach (string stage in item.GetStagesParticipated())
            {
                var list = GetStageList(stage);
                list.Add(item);
            }

            _distinctRenderItems.Add(item);
        }

        public void RemoveRenderItem(RenderItem item)
        {
            foreach (string stage in item.GetStagesParticipated())
            {
                GetStageList(stage).Remove(item);
            }

            _distinctRenderItems.Remove(item);
        }

        public void CollectVisibleObjects(RenderQueue queue, string pipelineStage, Vector3 position, Vector3 direction)
        {
            queue.AddRange(GetStageList(pipelineStage));
        }

        private List<RenderItem> GetStageList(string stage)
        {
            List<RenderItem> items;
            if (!_renderItemsByStage.TryGetValue(stage, out items))
            {
                items = new List<RenderItem>();
                _renderItemsByStage.Add(stage, items);
            }

            return items;
        }
    }
}
