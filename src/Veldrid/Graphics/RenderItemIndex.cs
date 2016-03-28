using System;

namespace Veldrid.Graphics
{
    public struct RenderItemIndex : IComparable<RenderItemIndex>, IComparable
    {
        public RenderOrderKey Key { get; }
        public int ItemIndex { get; }

        public RenderItemIndex(RenderOrderKey key, int itemIndex)
        {
            Key = key;
            ItemIndex = itemIndex;
        }

        public int CompareTo(object obj)
        {
            return ((IComparable)Key).CompareTo(obj);
        }

        public int CompareTo(RenderItemIndex other)
        {
            return Key.CompareTo(other.Key);
        }
    }
}
