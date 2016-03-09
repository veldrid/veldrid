using System;

namespace Veldrid.Graphics
{
    public struct RenderOrderKey : IComparable<RenderOrderKey>
    {
        private readonly long _rawKey;

        internal RenderOrderKey(long rawKey)
        {
            _rawKey = rawKey;
        }

        public int CompareTo(RenderOrderKey other)
        {
            return _rawKey.CompareTo(other._rawKey);
        }
    }
}
