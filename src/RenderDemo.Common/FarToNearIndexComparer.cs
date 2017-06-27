using System.Collections.Generic;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class FarToNearIndexComparer : Comparer<RenderItemIndex>
    {
        public override int Compare(RenderItemIndex x, RenderItemIndex y)
        {
            RenderOrderKey xKey = x.Key;
            RenderOrderKey yKey = y.Key;

            return -xKey.GetDistance().CompareTo(yKey.GetDistance());
        }
    }
}
