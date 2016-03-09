using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Graphics
{
    public class FlatListVisibilityManager : VisibiltyManager
    {
        private readonly List<RenderItem> _renderItems = new List<RenderItem>();

        public void AddRenderItem(RenderItem item) => _renderItems.Add(item);

        public void RemoveRenderItem(RenderItem item) => _renderItems.Remove(item);

        public void CollectVisibleObjects(RenderQueue queue, Vector3 position, Vector3 direction)
        {
            queue.AddRange(_renderItems);
        }
    }
}
