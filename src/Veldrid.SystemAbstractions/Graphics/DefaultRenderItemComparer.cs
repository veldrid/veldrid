using System.Collections.Generic;

namespace Veldrid.Graphics
{
    public class DefaultRenderItemComparer : Comparer<RenderItemIndex>
    {
        public override int Compare(RenderItemIndex x, RenderItemIndex y)
        {
            return x.CompareTo(y);
        }
    }
}