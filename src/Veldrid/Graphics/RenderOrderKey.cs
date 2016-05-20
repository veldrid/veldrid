using System;

namespace Veldrid.Graphics
{
    public struct RenderOrderKey : IComparable<RenderOrderKey>, IComparable
    {
        private readonly long _rawKey;

        public RenderOrderKey(long rawKey)
        {
            _rawKey = rawKey;
        }

        public int CompareTo(object obj)
        {
            return ((IComparable)_rawKey).CompareTo(obj);
        }

        public int CompareTo(RenderOrderKey other)
        {
            return _rawKey.CompareTo(other._rawKey);
        }
    }
}
