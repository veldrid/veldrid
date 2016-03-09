using System;

namespace Veldrid.Graphics
{
    public struct RenderItemIndex : IComparable<RenderOrderKey>
    {
        public RenderOrderKey Key { get; }
        public int ItemIndex { get; }

        public RenderItemIndex(RenderOrderKey key, int itemIndex)
        {
            Key = key;
            ItemIndex = itemIndex;
        }

        public int CompareTo(RenderOrderKey other)
        {
            return Key.CompareTo(other);
        }
    }
}
