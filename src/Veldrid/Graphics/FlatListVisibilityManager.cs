using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Veldrid.Graphics
{
    public class FlatListVisibilityManager : VisibiltyManager
    {
        private readonly Dictionary<string, List<RenderItem>> _renderItems = new Dictionary<string, List<RenderItem>>();

        public IEnumerable<RenderItem> RenderItems => _renderItems.SelectMany(kvp => kvp.Value);

        public void AddRenderItem(RenderItem item)
        {
            foreach (string stage in item.GetStagesParticipated())
            {
                var list = GetStageList(stage);
                list.Add(item);
            }
        }

        private List<RenderItem> GetStageList(string stage)
        {
            List<RenderItem> items;
            if (!_renderItems.TryGetValue(stage, out items))
            {
                items = new List<RenderItem>();
                _renderItems.Add(stage, items);
            }

            return items;
        }

        public void RemoveRenderItem(RenderItem item)
        {
            foreach (string stage in item.GetStagesParticipated())
            {
                GetStageList(stage).Remove(item);
            }
        }

        public void CollectVisibleObjects(RenderQueue queue, string pipelineStage, Vector3 position, Vector3 direction)
        {
            queue.AddRange(GetStageList(pipelineStage));
        }
    }
}
